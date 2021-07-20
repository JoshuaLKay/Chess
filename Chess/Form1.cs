using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace Chess
{
    public partial class Form1 : Form
    {
        //TODO
        //Finish end screen
        //Probably do optimisations to get depth above 5
        //Transposition Tables
        //Multithreading?
        Random rnd = new Random();
        int LastClick = 0;
        bool Player = true;
        //Stores the last piece the player clicked on
        int[] LastPiece = new int[2];
        //Stores the last piece black moved
        int[] BlackMove = new int[4] { 8, 8, 8, 8 };
        //Stores a grid of possible player moves for the clicked on piece
        int[,] PlayerMovesBoard = new int[8, 8];
        //piece values
        readonly int[] Values = new int[6] { 20000, 900, 500, 300, 300, 100 };
        //diagnostic stuff
        int TimeTaken = 0;
        int MovesChecked = 0;
        //starting board
        readonly int[,] InitialBoard = new int[8, 8] {
        { -3, -6, 0, 0, 0, 0, 6, 3 },
        { -5, -6, 0, 0, 0, 0, 6, 5 },
        { -4, -6, 0, 0, 0, 0, 6, 4 },
        { -2, -6, 0, 0, 0, 0, 6, 2 },
        { -1, -6, 0, 0, 0, 0, 6, 1 },
        { -4, -6, 0, 0, 0, 0, 6, 4 },
        { -5, -6, 0, 0, 0, 0, 6, 5 },
        { -3, -6, 0, 0, 0, 0, 6, 3 } };
        //from https://www.chessprogramming.org/Simplified_Evaluation_Function
        //these should be rotated 90 anti clockwise (Use [7-j, i] instead of [i, j]) this might be wrong I need to test it more (i think it works)
        readonly int[,,] PieceSquareTables = new int[6, 8, 8]
        //king
        { { { -30, -40, -40, -50, -50, -40, -40, -30 },
        { -30, -40, -40, -50, -50, -40, -40, -30 },
        { -30, -40, -40, -50, -50, -40, -40, -30 },
        { -30, -40, -40, -50, -50, -40, -40, -30 },
        { -20, -30, -30, -40, -40, -30, -30, -20 },
        { -10, -20, -20, -20, -20, -20, -20, -10 },
        { 20, 20, 0, 0, 0, 0, 20, 20 },
        { 20, 30, 10, 0, 0, 10, 30, 20 } },
        //queen
        { { -20, -10, -10, -5, -5, -10, -10, -20 },
        { -10, 0, 0, 0, 0, 0, 0, -10 },
        { -10, 0, 5, 5, 5, 5, 0, -10 },
        { -5, 0, 5, 5, 5, 5, 0, -5 },
        { 0, 0, 5, 5, 5, 5, 0, -5 },
        { -10, 5, 5, 5, 5, 5, 0, -10 },
        { -10, 0, 5, 0, 0, 0, 0, -10 },
        { -20, -10, -10, -5, -5, -10, -10, -20 } },
        //rook
        { { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 5, 10, 10, 10, 10, 10, 10, 5 },
        { -5, 0, 0, 0, 0, 0, 0, -5 },
        { -5, 0, 0, 0, 0, 0, 0, -5 },
        { -5, 0, 0, 0, 0, 0, 0, -5 },
        { -5, 0, 0, 0, 0, 0, 0, -5 },
        { -5, 0, 0, 0, 0, 0, 0, -5 },
        { 0, 0, 0, 5, 5, 0, 0, 0 } },
        //bishop
        { { -20, -10, -10, -10, -10, -10, -10, -20 },
        { -10, 0, 0, 0, 0, 0, 0, -10 },
        { -10, 0, 5, 10, 10, 5, 0, -10 },
        { -10, 5, 5, 10, 10, 5, 5, -10 },
        { -10, 0, 10, 10, 10, 10, 0, -10 },
        { -10, 10, 10, 10, 10, 10, 10, -10 },
        { -10, 5, 0, 0, 0, 0, 5, -10 },
        { -20, -10, -10, -10, -10, -10, -10, -20 } },
        //knight
        { { -50, -40, -30, -30, -30, -30, -40, -50 },
        { -40, -20, 0, 0, 0, 0, -20, -40 },
        { -30, 0, 10, 15, 15, 10, 0, -30 },
        { -30, 5, 15, 20, 20, 15, 5, -30 },
        { -30, 0, 15, 20, 20, 15, 0, -30 },
        { -30, 5, 10, 15, 15, 10, 5, -30 },
        { -40, -20, 0, 5, 5, 0, -20, -40 },
        { -50, -40, -30, -30, -30, -30, -40, -50 } },
        //pawn
        { { 0, 0, 0, 0, 0, 0, 0, 0 },
        { 50, 50, 50, 50, 50, 50, 50, 50 },
        { 10, 10, 20, 30, 30, 20, 10, 10 },
        { 5, 5, 10, 25, 25, 10, 5, 5 },
        { 0, 0, 0, 20, 20, 0, 0, 0 },
        { 5, -5, -10, 0, 0, -10, -5, 5 },
        { 5, 10, 10, -20, -20, 10, 10, 5 },
        { 0, 0, 0, 0, 0, 0, 0, 0 } } };
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ShowBoard(InitialBoard, new int[8, 8]);
        }
        //AI start
        private void AIMain(int[,] Board)
        {
            int Depth = 5;
            var watch = new Stopwatch();
            MovesChecked = 0;
            watch.Start();
            Board = NextMove(Board, MiniMaxMain(Depth, Board, false));
            watch.Stop();
            TimeTaken = Convert.ToInt32(watch.ElapsedMilliseconds);
            Player = true;
            ShowBoard(Board, new int[8, 8]);
        }
        private int[] MiniMaxMain(int Depth, int[,] Board, bool Side)
        {
            List<int[]> Moves = GetAllMoves(Board, Side);
            int BestValue = -1000000;
            int[] BestMove = Moves[0];
            for (int i = 0; i < Moves.Count; i++)
            {
                int[] NewMove = Moves[i];
                int[,] NewBoard = NextMove(Board, NewMove);
                var value = MiniMax(Depth - 1, NewBoard, !Side, -1000000, 1000000);
                if (value >= BestValue)
                {
                    BestValue = value;
                    BestMove = NewMove;
                }
            }
            BlackMove = BestMove;
            return BestMove;
        }
        private int MiniMax(int Depth, int[,] Board, bool Side, int Alpha, int Beta) //Make sure this works (I think i does now)
        {
            if (Depth == 0)
            {
                MovesChecked++;
                return -BoardValue(Board);
            }
            int BestMove;
            List<int[]> Moves = GetAllMoves(Board, Side);
            if (!Side)
            {
                //Max
                BestMove = -1000000;
                for (int i = 0; i < Moves.Count; i++)
                {
                    int[,] NewBoard = NextMove(Board, Moves[i]);
                    BestMove = Math.Max(BestMove, MiniMax(Depth - 1, NewBoard, !Side, Alpha, Beta));
                    Alpha = Math.Max(Alpha, BestMove);
                    //Alpha pruning
                    if (Beta <= Alpha)
                    {
                        return BestMove;
                    }
                }
            }
            else
            {
                //Min
                BestMove = 1000000;
                for (int i = 0; i < Moves.Count; i++)
                {
                    int[,] NewBoard = NextMove(Board, Moves[i]);
                    BestMove = Math.Min(BestMove, MiniMax(Depth - 1, NewBoard, !Side, Alpha, Beta));
                    //Beta pruning
                    Beta = Math.Min(Beta, BestMove);
                    if (Beta <= Alpha)
                    {
                        return BestMove;
                    }
                }
            }
            return BestMove;
        }
        //returns all moves
        private List<int[]> GetAllMoves(int[,] Board, bool Side)
        {
            List<int[]> PossibleMoves = new List<int[]>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    List<int[]> NextMove = LegalMove(i, j, Board[i, j], Board, Side);
                    PossibleMoves.AddRange(NextMove);
                }
            }
            return PossibleMoves;
        }
        //returns the value of the board from white's perspective
        private int BoardValue(int[,] Board)
        {
            int Value = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    //the values for PieceSquareTables are wierd because they are have to be rotated 90 anti clockwise from everything else (I will forget to change this later)
                    if (Board[i,j] > 0)
                    {
                        Value += Values[Board[i, j] - 1] + PieceSquareTables[Board[i, j] - 1, 7 - j, i];
                    }
                    if (Board[i,j] < 0)
                    {
                        Value -= Values[-Board[i, j] - 1] + PieceSquareTables[-Board[i, j] - 1, 7 - j, 7 - i];
                    }
                }
            }
            return Value;
        }
        //makes the move given to it, returns the updated board
        private int[,] NextMove(int[,] Board, int[] move)
        {
            int[,] NewBoard = new int[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    NewBoard[i, j] = Board[i, j];
                }
            }
            int CurrentPiece = NewBoard[move[0], move[1]];
            //pawn promotion
            if (move[3] == 0 && CurrentPiece == 6)
            {
                CurrentPiece = 2;
            }
            else if (move[3] == 7 && CurrentPiece == -6)
            {
                CurrentPiece = -2;
            }
            NewBoard[move[0], move[1]] = 0;
            NewBoard[move[2], move[3]] = CurrentPiece;
            return NewBoard;
        }
        //when pictureboxes are clicked on
        private void BoardClick(int[,] Board, string Name)
        {
            int x = Convert.ToInt32(Name.Substring(0, 1)); //(Cursor.Position.X - this.Left) / 100;
            int y = Convert.ToInt32(Name.Substring(1, 1)); //(Cursor.Position.Y - this.Top) / 100;
            //a list of all possible moves for the player
            List<int[]> PlayerMoves = new List<int[]>();
            if (x >= 0 && x <= 7 && y >= 0 && y <= 7)
            {
                if (LastClick == 0)
                {

                    if (Board[x, y] != 0)
                    {
                        PlayerMoves.AddRange(LegalMove(x, y, Board[x, y], Board, true));
                        for (int i = 0; i < PlayerMoves.Count; i++)
                        {
                            PlayerMovesBoard[PlayerMoves[i][2], PlayerMoves[i][3]] = 1;
                        }
                        LastPiece[0] = x;
                        LastPiece[1] = y;
                        LastClick = 1;
                        ShowBoard(Board, PlayerMovesBoard);
                    }
                }
                else
                {
                    if (PlayerMovesBoard[x, y] == 1)
                    {
                        int[,] PotentialBoard = NextMove(Board, new int[] { LastPiece[0], LastPiece[1], x, y });
                        if (KingCheck(PotentialBoard))
                        {
                            Board = PotentialBoard;
                            Player = false;
                        }
                    }
                    PlayerMovesBoard = new int[8, 8];
                    LastClick = 0;
                    PlayerMoves.Clear();
                    ShowBoard(Board, PlayerMovesBoard);
                }
            }
        }
        private bool KingCheck(int [,] Board)
        {
            List<int[]> Moves = GetAllMoves(Board, false);
            for (int i = 0; i < Moves.Count; i++)
            {
                //checks if any of the next possible black moves will capture the king
                if (!IsWhiteKing(NextMove(Board, Moves[i])))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CheckMate(int[,] Board)
        {
            List<int[]> Moves = GetAllMoves(Board, true);
            for (int i = 0; i < Moves.Count; i++)
            {
                //checks if any of the next possible white moves will get out of check
                if (KingCheck(NextMove(Board, Moves[i])))
                {
                    return false;
                }
            }
            return true;
        }
        private void GameEnd(bool Winner) //could make look nicer
        {
            //Text Box
            Label Info = new Label();
            Info.ForeColor = Color.Black;
            Info.Location = new Point(800, 100);
            if (Winner)
            {
                Info.Text = "You win";
            }
            else
            {
                Info.Text = "Checkmate";
            }
            Controls.Add(Info);
            //Play Again
            Button PlayAgain = new Button();
            PlayAgain.Location = new Point(800, 125);
            PlayAgain.ForeColor = Color.Black;
            PlayAgain.Height = 50;
            PlayAgain.Width = 100;
            PlayAgain.Text = "Play Again";
            PlayAgain.MouseClick += new MouseEventHandler((o, a) => RestartGame());
            Controls.Add(PlayAgain);
            //End Game
            Button EndGame = new Button();
            EndGame.Location = new Point(900, 125);
            EndGame.ForeColor = Color.Black;
            EndGame.Height = 50;
            EndGame.Width = 100;
            EndGame.Text = "Close Game";
            EndGame.MouseClick += new MouseEventHandler((o, a) => CloseProgram());
            Controls.Add(EndGame);
        }
        private void RestartGame()
        {
            TimeTaken = 0;
            MovesChecked = 0;
            Player = true;
            ShowBoard(InitialBoard, new int[8, 8]);
        }
        private void CloseProgram()
        {
            this.Close();
        }
        private void ClearAll()
        {
            for (int i = Controls.Count - 1; i >= 0; i--)
            {
                Controls[i].Dispose();
            }
        }
        private bool IsWhiteKing(int[,] Board)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (Board[i, j] == 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private bool IsBlackKing(int[,] Board)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (Board[i, j] == -1)
                    {
                        return true;
                    }  
                }
            }
            return false;
        }
        //display chess board, also sets up pictureboxes with onclick and starts the ai after called after a player move (should probably move that latter stuff to somewhere else)
        private void ShowBoard(int[,] Board, int[,] PlayerMovesBoard)
        {
            //delete all imageboxes
            ClearAll();
            if (CheckMate(Board))
            {
                GameEnd(false);
            }
            //diagnostics
            //positions
            Label Info1 = new Label();
            Info1.Text = "Positions Analysed:";
            Info1.ForeColor = Color.Black;
            Info1.Location = new Point(800, 0);
            Controls.Add(Info1);
            Label Info2 = new Label();
            Info2.Text = Convert.ToString(MovesChecked);
            Info2.ForeColor = Color.Black;
            Info2.Location = new Point(900, 0);
            Controls.Add(Info2);
            //time taken
            Label Info3 = new Label();
            Info3.Text = "Time Taken (ms):";
            Info3.ForeColor = Color.Black;
            Info3.Location = new Point(800, 25);
            Controls.Add(Info3);
            Label Info4 = new Label();
            Info4.Text = Convert.ToString(TimeTaken);
            Info4.ForeColor = Color.Black;
            Info4.Location = new Point(900, 25);
            Controls.Add(Info4);
            if (!KingCheck(Board))
            {
                Label Check = new Label();
                Check.Text = "In Check";
                Check.ForeColor = Color.Black;
                Check.Location = new Point(800, 75);
            }
            //Piece display
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    //Draw Pieces
                    PictureBox NewPiece = new PictureBox();
                    //pieces.Add(NewPiece);
                    if ((i + j) % 2 == 0)
                    {
                        if (PlayerMovesBoard[i, j] == 1)
                        {
                            //white board, player attacking
                            switch (Board[i, j])
                            {
                                case (0):
                                    NewPiece.BackgroundImage = Properties.Resources.ChessSquareWhiteRed;
                                    break;
                                case (1):
                                    NewPiece.BackgroundImage = Properties.Resources.wkwr;
                                    break;
                                case (2):
                                    NewPiece.BackgroundImage = Properties.Resources.wqwr;
                                    break;
                                case (3):
                                    NewPiece.BackgroundImage = Properties.Resources.wrwr;
                                    break;
                                case (4):
                                    NewPiece.BackgroundImage = Properties.Resources.wbwr;
                                    break;
                                case (5):
                                    NewPiece.BackgroundImage = Properties.Resources.wnwr;
                                    break;
                                case (6):
                                    NewPiece.BackgroundImage = Properties.Resources.wpwr;
                                    break;
                                case (-1):
                                    NewPiece.BackgroundImage = Properties.Resources.bkwr;
                                    break;
                                case (-2):
                                    NewPiece.BackgroundImage = Properties.Resources.bqwr;
                                    break;
                                case (-3):
                                    NewPiece.BackgroundImage = Properties.Resources.brwr;
                                    break;
                                case (-4):
                                    NewPiece.BackgroundImage = Properties.Resources.bbwr;
                                    break;
                                case (-5):
                                    NewPiece.BackgroundImage = Properties.Resources.bnwr;
                                    break;
                                case (-6):
                                    NewPiece.BackgroundImage = Properties.Resources.bpwr;
                                    break;
                            }
                        }
                        else
                        {
                            //whte board normal
                            switch (Board[i, j])
                            {
                                case (0):
                                    NewPiece.BackgroundImage = Properties.Resources.ChessSquareWhite;
                                    break;
                                case (1):
                                    NewPiece.BackgroundImage = Properties.Resources.wkw;
                                    break;
                                case (2):
                                    NewPiece.BackgroundImage = Properties.Resources.wqw;
                                    break;
                                case (3):
                                    NewPiece.BackgroundImage = Properties.Resources.wrw;
                                    break;
                                case (4):
                                    NewPiece.BackgroundImage = Properties.Resources.wbw;
                                    break;
                                case (5):
                                    NewPiece.BackgroundImage = Properties.Resources.wnw;
                                    break;
                                case (6):
                                    NewPiece.BackgroundImage = Properties.Resources.wpw;
                                    break;
                                case (-1):
                                    NewPiece.BackgroundImage = Properties.Resources.bkw;
                                    break;
                                case (-2):
                                    NewPiece.BackgroundImage = Properties.Resources.bqw;
                                    break;
                                case (-3):
                                    NewPiece.BackgroundImage = Properties.Resources.brw;
                                    break;
                                case (-4):
                                    NewPiece.BackgroundImage = Properties.Resources.bbw;
                                    break;
                                case (-5):
                                    NewPiece.BackgroundImage = Properties.Resources.bnw;
                                    break;
                                case (-6):
                                    NewPiece.BackgroundImage = Properties.Resources.bpw;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (PlayerMovesBoard[i, j] == 1)
                        {
                            //black board, player attacking
                            switch (Board[i, j])
                            {
                                case (0):
                                    NewPiece.BackgroundImage = Properties.Resources.ChessSquareBlackRed;
                                    break;
                                case (1):
                                    NewPiece.BackgroundImage = Properties.Resources.wkbr;
                                    break;
                                case (2):
                                    NewPiece.BackgroundImage = Properties.Resources.wqbr;
                                    break;
                                case (3):
                                    NewPiece.BackgroundImage = Properties.Resources.wrbr;
                                    break;
                                case (4):
                                    NewPiece.BackgroundImage = Properties.Resources.wbbr;
                                    break;
                                case (5):
                                    NewPiece.BackgroundImage = Properties.Resources.wnbr;
                                    break;
                                case (6):
                                    NewPiece.BackgroundImage = Properties.Resources.wpbr;
                                    break;
                                case (-1):
                                    NewPiece.BackgroundImage = Properties.Resources.bkbr;
                                    break;
                                case (-2):
                                    NewPiece.BackgroundImage = Properties.Resources.bqbr;
                                    break;
                                case (-3):
                                    NewPiece.BackgroundImage = Properties.Resources.brbr;
                                    break;
                                case (-4):
                                    NewPiece.BackgroundImage = Properties.Resources.bbbr;
                                    break;
                                case (-5):
                                    NewPiece.BackgroundImage = Properties.Resources.bnbr;
                                    break;
                                case (-6):
                                    NewPiece.BackgroundImage = Properties.Resources.bpbr;
                                    break;
                            }
                        }
                        else
                        {
                            //black board normal
                            switch (Board[i, j])
                            {
                                case (0):
                                    NewPiece.BackgroundImage = Properties.Resources.ChessSquareBlack;
                                    break;
                                case (1):
                                    NewPiece.BackgroundImage = Properties.Resources.wkb;
                                    break;
                                case (2):
                                    NewPiece.BackgroundImage = Properties.Resources.wqb;
                                    break;
                                case (3):
                                    NewPiece.BackgroundImage = Properties.Resources.wrb;
                                    break;
                                case (4):
                                    NewPiece.BackgroundImage = Properties.Resources.wbb;
                                    break;
                                case (5):
                                    NewPiece.BackgroundImage = Properties.Resources.wnb;
                                    break;
                                case (6):
                                    NewPiece.BackgroundImage = Properties.Resources.wpb;
                                    break;
                                case (-1):
                                    NewPiece.BackgroundImage = Properties.Resources.bkb;
                                    break;
                                case (-2):
                                    NewPiece.BackgroundImage = Properties.Resources.bqb;
                                    break;
                                case (-3):
                                    NewPiece.BackgroundImage = Properties.Resources.brb;
                                    break;
                                case (-4):
                                    NewPiece.BackgroundImage = Properties.Resources.bbb;
                                    break;
                                case (-5):
                                    NewPiece.BackgroundImage = Properties.Resources.bnb;
                                    break;
                                case (-6):
                                    NewPiece.BackgroundImage = Properties.Resources.bpb;
                                    break;
                            }
                        }
                    }
                    //shows the last move black made
                    if (BlackMove[0] == i && BlackMove[1] == j)
                    {
                        if ((i + j) % 2 == 0)
                        {
                            NewPiece.BackgroundImage = Properties.Resources.ChessSquareWhiteRed;
                        }
                        else
                        {
                            NewPiece.BackgroundImage = Properties.Resources.ChessSquareBlackRed;
                        }
                        BlackMove[0] = 8;
                        BlackMove[1] = 8;
                    }
                    if (BlackMove[2] == i && BlackMove[3] == j)
                    {
                        if ((i + j) % 2 == 0)
                        {
                            switch (Board[i, j])
                            {
                                case (-1):
                                    NewPiece.BackgroundImage = Properties.Resources.bkwr;
                                    break;
                                case (-2):
                                    NewPiece.BackgroundImage = Properties.Resources.bqwr;
                                    break;
                                case (-3):
                                    NewPiece.BackgroundImage = Properties.Resources.brwr;
                                    break;
                                case (-4):
                                    NewPiece.BackgroundImage = Properties.Resources.bbwr;
                                    break;
                                case (-5):
                                    NewPiece.BackgroundImage = Properties.Resources.bnwr;
                                    break;
                                case (-6):
                                    NewPiece.BackgroundImage = Properties.Resources.bpwr;
                                    break;
                            }
                        }
                        else
                        {
                            switch (Board[i, j])
                            {
                                case (-1):
                                    NewPiece.BackgroundImage = Properties.Resources.bkbr;
                                    break;
                                case (-2):
                                    NewPiece.BackgroundImage = Properties.Resources.bqbr;
                                    break;
                                case (-3):
                                    NewPiece.BackgroundImage = Properties.Resources.brbr;
                                    break;
                                case (-4):
                                    NewPiece.BackgroundImage = Properties.Resources.bbbr;
                                    break;
                                case (-5):
                                    NewPiece.BackgroundImage = Properties.Resources.bnbr;
                                    break;
                                case (-6):
                                    NewPiece.BackgroundImage = Properties.Resources.bpbr;
                                    break;
                            } 
                        }
                        BlackMove[2] = 8;
                        BlackMove[3] = 8;
                    }
                    NewPiece.BackColor = Color.Transparent;
                    NewPiece.Height = 100;
                    NewPiece.Width = 100;
                    NewPiece.Location = new Point(i * 100, j * 100);
                    NewPiece.Name = i + "" + j;
                    NewPiece.MouseClick += new MouseEventHandler((o, a) => BoardClick(Board, NewPiece.Name));
                    Controls.Add(NewPiece);
                }
            }
            if (!IsBlackKing(Board))
            {
                GameEnd(true);
            }
            if (!IsWhiteKing(Board))
            {
                GameEnd(false);
            }
            if (!Player)
            {
                this.Update();
                AIMain(Board);
            }
        }
        //returns a list of all legal moves for one piece
        private List<int[]> LegalMove(int x, int y, int piece, int[,] Board, bool Side)
        {
            List<int[]> moves = new List<int[]>();
            if (Side)
            {
                switch (piece)
                {
                    //White King
                    case (1):
                        moves.AddRange(KingMoves(x, y, 0, Board));
                        break;
                    //White Queen
                    case (2):
                        moves.AddRange(QueenMoves(x, y, 0, Board));
                        break;
                    //White Rook
                    case (3):
                        moves.AddRange(RookMoves(x, y, 0, Board));
                        break;
                    //White Bishop
                    case (4):
                        moves.AddRange(BishopMoves(x, y, 0, Board));
                        break;
                    //White Knight
                    case (5):
                        moves.AddRange(KnightMoves(x, y, 0, Board));
                        break;
                    //White Pawn
                    case (6):
                        moves.AddRange(PawnMoves(x, y, 0, Board));
                        break;
                }
            }
            else
            {
                switch (piece)
                {
                    //Black King
                    case (-1):
                        moves.AddRange(KingMoves(x, y, 1, Board));
                        break;
                    //Black Queen
                    case (-2):
                        moves.AddRange(QueenMoves(x, y, 1, Board));
                        break;
                    //Black Rook
                    case (-3):
                        moves.AddRange(RookMoves(x, y, 1, Board));
                        break;
                    //Black Bishop
                    case (-4):
                        moves.AddRange(BishopMoves(x, y, 1, Board));
                        break;
                    //Black Knight
                    case (-5):
                        moves.AddRange(KnightMoves(x, y, 1, Board));
                        break;
                    //Black Pawn
                    case (-6):
                        moves.AddRange(PawnMoves(x, y, 1, Board));
                        break;
                }
            }
            return moves;
        }
        //legal moves
        //king - needs castling
        private List<int[]> KingMoves(int x, int y, int color, int[,] Board)
        {
            List<int[]> moves = new List<int[]>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (x + i >= 0 && x + i <= 7 && y + j >= 0 && y + j <= 7)
                    {
                        if (i != 0 || j != 0)
                        {
                            switch (color)
                            {
                                case (0):
                                    if (Board[x + i, y + j] <= 0)
                                    {
                                        int[] NewMove = new int[] { x, y, x + i, y + j };
                                        moves.Add(NewMove);
                                    }
                                    break;
                                case (1):
                                    if (Board[x + i, y + j] >= 0)
                                    {
                                        int[] NewMove = new int[] { x, y, x + i, y + j };
                                        moves.Add(NewMove);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            return moves;
        }
        //queen
        private List<int[]> QueenMoves(int x, int y, int color, int[,] Board)
        {
            List<int[]> moves = new List<int[]>();
            moves.AddRange(RookMoves(x, y, color, Board));
            moves.AddRange(BishopMoves(x, y, color, Board));
            return moves;
        }
        //rook
        private List<int[]> RookMoves(int x, int y, int color, int[,] Board)
        {
            List<int[]> moves = new List<int[]>();
            //Up
            for (int i = -1; i >= -7; i--)
            {
                if (y + i >= 0 && y + i <= 7)
                {
                    switch (color)
                    {
                        case (0):
                            if (Board[x, y + i] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x, y + i };
                                moves.Add(NewMove);
                            }
                            else if (Board[x, y + i] > 0)
                            {
                                i = -8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x, y + i };
                                moves.Add(NewMove);
                                i = -8;
                            }
                            break;
                        case (1):
                            if (Board[x, y + i] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x, y + i };
                                moves.Add(NewMove);
                            }
                            else if (Board[x, y + i] < 0)
                            {
                                i = -8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x, y + i };
                                moves.Add(NewMove);
                                i = -8;
                            }
                            break;
                    }
                }
            }
            //Down
            for (int i = 1; i <= 7; i++)
            {
                if (y + i >= 0 && y + i <= 7)
                {
                    switch (color)
                    {
                        case (0):
                            if (Board[x, y + i] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x, y + i };
                                moves.Add(NewMove);
                            }
                            else if (Board[x, y + i] > 0)
                            {
                                i = 8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x, y + i };
                                moves.Add(NewMove);
                                i = 8;
                            }
                            break;
                        case (1):
                            if (Board[x, y + i] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x, y + i };
                                moves.Add(NewMove);
                            }
                            else if (Board[x, y + i] < 0)
                            {
                                i = 8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x, y + i };
                                moves.Add(NewMove);
                                i = 8;
                            }
                            break;
                    }
                }
            }
            //Left
            for (int i = -1; i >= -7; i--)
            {
                if (x + i >= 0 && x + i <= 7)
                {
                    switch (color)
                    {
                        case (0):
                            if (Board[x + i, y] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x + i, y };
                                moves.Add(NewMove);
                            }
                            else if (Board[x + i, y] > 0)
                            {
                                i = -8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x + i, y };
                                moves.Add(NewMove);
                                i = -8;
                            }
                            break;
                        case (1):
                            if (Board[x + i, y] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x + i, y };
                                moves.Add(NewMove);
                            }
                            else if (Board[x + i, y] < 0)
                            {
                                i = -8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x + i, y };
                                moves.Add(NewMove);
                                i = -8;
                            }
                            break;
                    }
                }
            }
            //Right
            for (int i = 1; i <= 7; i++)
            {
                if (x + i >= 0 && x + i <= 7)
                {
                    switch (color)
                    {
                        case (0):
                            if (Board[x + i, y] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x + i, y };
                                moves.Add(NewMove);
                            }
                            else if (Board[x + i, y] > 0)
                            {
                                i = 8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x + i, y };
                                moves.Add(NewMove);
                                i = 8;
                            }
                            break;
                        case (1):
                            if (Board[x + i, y] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x + i, y };
                                moves.Add(NewMove);
                            }
                            else if (Board[x + i, y] < 0)
                            {
                                i = 8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x + i, y };
                                moves.Add(NewMove);
                                i = 8;
                            }
                            break;
                    }
                }
            }
            return moves;
        }
        //bishop
        private List<int[]> BishopMoves(int x, int y, int color, int[,] Board)
        {
            List<int[]> moves = new List<int[]>();
            //Up Left
            for (int i = -1; i >= -7; i--)
            {
                if (x + i >= 0 && x + i <= 7 && y + i >= 0 && y + i <= 7)
                {
                    switch (color)
                    {
                        case (0):
                            if (Board[x + i, y + i] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x + i, y + i };
                                moves.Add(NewMove);
                            }
                            else if (Board[x + i, y + i] > 0)
                            {
                                i = -8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x + i, y + i };
                                moves.Add(NewMove);
                                i = -8;
                            }
                            break;
                        case (1):
                            if (Board[x + i, y + i] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x + i, y + i };
                                moves.Add(NewMove);
                            }
                            else if (Board[x + i, y + i] < 0)
                            {
                                i = -8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x + i, y + i };
                                moves.Add(NewMove);
                                i = -8;
                            }
                            break;
                    }
                }
            }
            //Up Right
            for (int i = -1; i >= -7; i--)
            {
                if (x - i >= 0 && x - i <= 7 && y + i >= 0 && y + i <= 7)
                {
                    switch (color)
                    {
                        case (0):
                            if (Board[x - i, y + i] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x - i, y + i };
                                moves.Add(NewMove);
                            }
                            else if (Board[x - i, y + i] > 0)
                            {
                                i = -8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x - i, y + i };
                                moves.Add(NewMove);
                                i = -8;
                            }
                            break;
                        case (1):
                            if (Board[x - i, y + i] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x - i, y + i };
                                moves.Add(NewMove);
                            }
                            else if (Board[x - i, y + i] < 0)
                            {
                                i = -8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x - i, y + i };
                                moves.Add(NewMove);
                                i = -8;
                            }
                            break;
                    }
                }
            }
            //Down Left
            for (int i = -1; i >= -7; i--)
            {
                if (x + i >= 0 && x + i <= 7 && y - i >= 0 && y - i <= 7)
                {
                    switch (color)
                    {
                        case (0):
                            if (Board[x + i, y - i] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x + i, y - i };
                                moves.Add(NewMove);
                            }
                            else if (Board[x + i, y - i] > 0)
                            {
                                i = -8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x + i, y - i };
                                moves.Add(NewMove);
                                i = -8;
                            }
                            break;
                        case (1):
                            if (Board[x + i, y - i] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x + i, y - i };
                                moves.Add(NewMove);
                            }
                            else if (Board[x + i, y - i] < 0)
                            {
                                i = -8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x + i, y - i };
                                moves.Add(NewMove);
                                i = -8;
                            }
                            break;
                    }
                }
            }
            //Down Right
            for (int i = -1; i >= -7; i--)
            {
                if (x - i >= 0 && x - i <= 7 && y - i >= 0 && y - i <= 7)
                {
                    switch (color)
                    {
                        case (0):
                            if (Board[x - i, y - i] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x - i, y - i };
                                moves.Add(NewMove);
                            }
                            else if (Board[x - i, y - i] > 0)
                            {
                                i = -8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x - i, y - i };
                                moves.Add(NewMove);
                                i = -8;
                            }
                            break;
                        case (1):
                            if (Board[x - i, y - i] == 0)
                            {
                                int[] NewMove = new int[] { x, y, x - i, y - i };
                                moves.Add(NewMove);
                            }
                            else if (Board[x - i, y - i] < 0)
                            {
                                i = -8;
                            }
                            else
                            {
                                int[] NewMove = new int[] { x, y, x - i, y - i };
                                moves.Add(NewMove);
                                i = -8;
                            }
                            break;
                    }
                }
            }
            return moves;
        }
        //knight
        private List<int[]> KnightMoves(int x, int y, int color, int[,] Board)
        {
            List<int[]> moves = new List<int[]>();
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    if ((Math.Abs(i) == Math.Abs(j) + 1 || Math.Abs(i) + 1 == Math.Abs(j)) && (i != 0 && j != 0))
                    {
                        if (x + i >= 0 && x + i <= 7 && y + j >= 0 && y + j <= 7)
                        {
                            switch (color)
                            {
                                case (0):
                                    if (Board[x + i, y + j] <= 0)
                                    {
                                        int[] NewMove = new int[] { x, y, x + i, y + j };
                                        moves.Add(NewMove);
                                    }
                                    break;
                                case (1):
                                    if (Board[x + i, y + j] >= 0)
                                    {
                                        int[] NewMove = new int[] { x, y, x + i, y + j };
                                        moves.Add(NewMove);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            return moves;
        }
        //pawn - needs en passant
        private List<int[]> PawnMoves(int x, int y, int color, int[,] Board)
        {
            List<int[]> moves = new List<int[]>();
            switch (color)
            {
                //White
                case (0):
                    //normal move
                    if (y - 1 >= 0)
                    {
                        if (Board[x, y - 1] == 0)
                        {
                            int[] NewMove = new int[] { x, y, x, y - 1 };
                            moves.Add(NewMove);
                        }
                    }
                    //first move
                    if (y == 6)
                    {
                        if (Board[x, y - 1] == 0 && Board[x, y - 2] == 0)
                        {
                            int[] NewMove = new int[] { x, y, x, y - 2 };
                            moves.Add(NewMove);
                        }
                    }
                    //take right
                    if (y - 1 >= 0 && x + 1 <= 7)
                    {
                        if (Board[x + 1, y - 1] < 0)
                        {
                            int[] NewMove = new int[] { x, y, x + 1, y - 1 };
                            moves.Add(NewMove);
                        }
                    }
                    //take left
                    if (y - 1 >= 0 && x - 1 >= 0)
                    {
                        if (Board[x - 1, y - 1] < 0)
                        {
                            int[] NewMove = new int[] { x, y, x - 1, y - 1 };
                            moves.Add(NewMove);
                        }
                    }
                    break;
                //Black
                case (1):
                    //normal move
                    if (y + 1 <= 7)
                    {
                        if (Board[x, y + 1] == 0)
                        {
                            int[] NewMove = new int[] { x, y, x, y + 1 };
                            moves.Add(NewMove);
                        }
                    }
                    //first move
                    if (y == 1)
                    {
                        if (Board[x, y + 1] == 0 && Board[x, y + 2] == 0)
                        {
                            int[] NewMove = new int[] { x, y, x, y + 2 };
                            moves.Add(NewMove);
                        }
                    }
                    //take right
                    if (y + 1 <= 7 && x + 1 <= 7)
                    {
                        if (Board[x + 1, y + 1] > 0)
                        {
                            int[] NewMove = new int[] { x, y, x + 1, y + 1 };
                            moves.Add(NewMove);
                        }
                    }
                    //take left
                    if (y + 1 <= 7 && x - 1 >= 0)
                    {
                        if (Board[x - 1, y + 1] > 0)
                        {
                            int[] NewMove = new int[] { x, y, x - 1, y + 1 };
                            moves.Add(NewMove);
                        }
                    }
                    break;
            }
            return moves;
        }
    }
}
