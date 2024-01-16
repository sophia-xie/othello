using System;
using static System.Console;

namespace Othello
{
    static partial class Program
    {
        static void Main()
        {
            Console.Clear();

            //user customization
            (Player p1, Player p2) = GetPlayerInfo();
            (int rows, int cols) = GetBoardSize();

            //game setup
            Board board = new Board(rows, cols, p1, p2);
            Player currPlayer = p1;
            Player opponent = p2;
            bool endGame = false;
            bool couldNotMove = false;

            //gameplay
            while (!endGame)
            {
                //draw scoreboard and gameboard
                Console.Clear();
                Scoreboard(p1, p2);
                board.Display();

                //check if game is over (neither player can move)
                if (!board.CanMove(currPlayer, opponent) && !board.CanMove(opponent, currPlayer)) break;

                //display instructions
                Instructions();
                System.Threading.Thread.Sleep(250);

                //if opponent previously had no possible moves
                if (couldNotMove)
                {
                    WriteInColour(opponent.Colour, " {0}", opponent.Name);
                    WriteLine("'s turn was skipped because they had no possible moves.");
                    couldNotMove = false;
                }

                //check if current player can move
                if (!board.CanMove(currPlayer, opponent)) couldNotMove = true;

                //prompt current player to enter their move
                WriteInColour(currPlayer.Colour, " {0} ({1})", currPlayer.Name, currPlayer.Symbol);
                Write("'s turn: ");

                while (!couldNotMove)
                {
                    //collect move (convert to lowercase and remove any spaces)
                    string playerMove = String.Concat(ReadLine()!.ToLower().Where(c => !Char.IsWhiteSpace(c)));

                    if (playerMove == "quit")  //quit game
                    {
                        endGame = true;
                        break;
                    }
                    if (playerMove == "pass") break;  //pass turn

                    //make move if valid
                    string moveValidMessage = board.MakeMove(currPlayer, playerMove, opponent, false);
                    if (moveValidMessage == "valid") break;

                    //display reason for invalid move, prompt user to try again
                    Write(" {0} Please try again: ", moveValidMessage);
                }

                //switch turns
                Player temp = currPlayer;
                currPlayer = opponent;
                opponent = temp;
            }

            //end of game
            GameOver(p1, p2);
        }
    }
    class DiscClass : IEquatable<DiscClass>  //discs will be checked for equality
    {
        public char Symbol { get; private set; }
        public ConsoleColor Colour { get; private set; }
        public DiscClass(char symbol, ConsoleColor colour)
        {
            Symbol = symbol;
            Colour = colour;
        }
        public bool Equals(DiscClass? other)  //check if equal to another disc object
        {
            return other != null && Symbol == other.Symbol && Colour == other.Colour;
        }

        public override bool Equals(object? obj)  //override default object.Equals method
        {
            return Equals(obj as DiscClass);
        }

        public override int GetHashCode()  //keep .Equals and .GetHashCode() synchronized
        {
            return HashCode.Combine(Symbol, Colour);
        }
    }
    class Player : DiscClass  //inherit from disc to get player symbol and colour
    {
        public string Name { get; private set; }
        public int Score { get; set; }
        public DiscClass Disc { get; private set; }

        //constructor
        public Player(string name, char symbol, ConsoleColor colour) : base(symbol, colour)
        {
            Name = name;
            Disc = new DiscClass(Symbol, Colour);
            Score = 2;  //each player starts with 2 discs
        }
    }
    class Board
    {
        public int Rows {get; private set;}
        public int Cols {get; private set;}
        public DiscClass[,] Cells {get; set;}
        DiscClass emptyDisc = new DiscClass(' ', ConsoleColor.White);

        //constructor
        public Board(int rows, int cols, Player p1, Player p2)
        {
            Rows = rows;
            Cols = cols;
            Cells = new DiscClass[Rows, Cols];

            //fill with empty discs
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++) Cells[i, j] = emptyDisc;
            }

            //game setup: 4 discs at the centre
            Cells[Rows / 2 - 1, Cols / 2] = p1.Disc;  //north-east
            Cells[Rows / 2, Cols / 2 - 1] = p1.Disc;  //south-west
            Cells[Rows / 2 - 1, Cols / 2 - 1] = p2.Disc;  //north-west
            Cells[Rows / 2, Cols / 2] = p2.Disc;  //south-east
        }

        //draw gameboard on console
        public void Display()
        {
            //unicode symbols
            const char h = '\u2500';  //horizontal line
            const char v = '\u2502';  //vertical line
            const char tl = '\u250c';  // top left corner
            const char tr = '\u2510';  // top right corner
            const char bl = '\u2514';  // bottom left corner
            const char br = '\u2518';  // bottom right corner
            const char le = '\u251c';  // left edge
            const char re = '\u2524';  // right edge
            const char te = '\u252c';  // top edge
            const char be = '\u2534';  // bottom edge
            const char c = '\u253c';  // cross
            const char mc = '\u256c';  // marked cross

            //label columns
            Write("  ");
            for (int j = 0; j < Cols; j++) Write("   {0}", Program.IndexToLetter(j));
            WriteLine();

            //draw board row by row
            for (int i = 0; i < Rows; i++)
            {
                if (i == 0) //top row
                {
                    Write("   {0}{1}{1}{1}", tl, h);  //top left corner
                    for (int j = 1; j < Cols; j++) Write("{0}{1}{1}{1}", te, h);
                    WriteLine(tr);  //top right corner
                }
                else
                {
                    Write("   {0}{1}{1}{1}", le, h);  //left edge
                    for (int j = 1; j < Cols; j++)
                    {
                        if (i == 2 && (j == 2 || j == Cols - 2) || i == Rows - 2 && (j == 2 || j == Cols - 2))  //special marked crosses
                        {
                            Write("{0}{1}{1}{1}", mc, h);
                        }
                        else Write("{0}{1}{1}{1}", c, h);
                    }
                    WriteLine(re);  //right edge
                }

                //display row content column by column
                Write(" {0}", Program.IndexToLetter(i));  //label row
                for (int j = 0; j < Cols; j++)
                {
                    Write(" {0}", v);
                    Program.WriteInColour(Cells[i, j].Colour, " {0}", Cells[i, j].Symbol);  //draw disc
                }
                WriteLine(" {0}", v);
            }

            //border below bottom row
            Write("   {0}{1}{1}{1}", bl, h);  //bottom left corner
            for (int j = 1; j < Cols; j++) Write("{0}{1}{1}{1}", be, h);
            WriteLine(br);  //bottom right corner
        }

		//if valid move, return "valid"; if not, return reason why invalid
		public string MakeMove(Player currPlayer, string move, Player opponent, bool checkOnly)
		{
			string result = "No discs were flipped.";  //return value

			if (move.Length != 2) return ("Command must be 2 characters long.");  //move must be 2 letters

			int chosenRow = Program.LetterToIndex(move[0]);  //first letter is row
			int chosenCol = Program.LetterToIndex(move[1]);  //second letter is column

			if (chosenRow < 0 || chosenRow >= Rows || chosenCol < 0 || chosenCol >= Cols)  //cell not on board
			{
				return ("That cell is not on game board.");
			}

			if (Cells[chosenRow, chosenCol] != emptyDisc)  //cell not empty
			{
				return ("That cell is already taken.");
			}

			//define each of the 8 directions in terms of row and column
			//directions: upleft, up, upright, right, downright, down, downleft, left
			int[] rowDirections = { -1, -1, -1, 0, 1, 1, 1, 0 };
			int[] colDirections = { -1, 0, 1, 1, 1, 0, -1, -1 };

			//for each direction, check if any discs are flipped
			for (int i = 0; i < 8; i++)
            {
				//get adjacent cell
				int currRow = chosenRow + rowDirections[i];
				int currCol = chosenCol + colDirections[i];

				//avoid checking off the board
				if (currRow < 0 || currRow >= Rows || currCol < 0 || currCol >= Cols) continue;  //proceed to next direction

				//check if adjacent cell is opponent's disc
				if (Cells[currRow, currCol].Equals(opponent.Disc))
                {
					//search for current player's disc
					currRow += rowDirections[i];
					currCol += colDirections[i];

					//keep checking until off the board
					while (currRow >= 0 && currRow < Rows && currCol >= 0 && currCol < Cols)
                    {
						if (Cells[currRow, currCol].Equals(currPlayer.Disc))  //valid move detected (opponent's discs are surrounded)
						{
							result = "valid";
							if (checkOnly) return result;  //if only checking whether move valid, do not modify game board

							//flip opponent's discs in reverse order
							currRow -= rowDirections[i];
							currCol -= colDirections[i];
							while (currRow != chosenRow || currCol != chosenCol)  //continue until location of new disc is reached
							{
								Cells[currRow, currCol] = currPlayer.Disc;
								currRow -= rowDirections[i];
								currCol -= colDirections[i];

								//update scores
								currPlayer.Score++;
								opponent.Score--;
							}
							break;  //move on to next direction
						}

						//empty cell encountered, skip to next direction
						if (Cells[currRow, currCol].Equals(emptyDisc)) break;

						//another opponent disc encountered, proceed to check next cell in same direction
						currRow += rowDirections[i];
						currCol += colDirections[i];
					}
				}
			}

            //if valid move (opponent discs were flipped), place new disc and increase score by 1
            if (result == "valid")
            {
                Cells[chosenRow, chosenCol] = currPlayer.Disc;
                currPlayer.Score++;
                return result;
            }

            //otherwise, invalid move
            return "No pieces were flipped.";
		}

        //check if player can make a move
        public bool CanMove(Player currPlayer, Player opponent)
        {
            //loop through all cells
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    //get cell row and column letters
                    string cell = Program.IndexToLetter(i).ToString() + Program.IndexToLetter(j).ToString();

                    //check if valid move for this cell
                    //call MakeMove() function but set 'checkOnly' to true
                    if (MakeMove(currPlayer, cell, opponent, true) == "valid") return true;
                }
            }

            //no possible moves, player must pass
            return false;
        }
    }

    static partial class Program
    {
        //collect player information
        static (Player p1, Player p2) GetPlayerInfo()
        {
            //list of colours
            List<string> colours = new List<string> {"Green", "Yellow", "Magenta", "Cyan", "Red", "Blue"};

            WriteLine();
            WriteLine(" Welcome to Othello!");
            WriteLine();
            WriteLine(" Start by customizing your game.");
            WriteLine();

            //player 1
            WriteLine(" PLAYER 1");
            WriteLine(" --------");

            //player 1's name
            string p1Name = "Player 1";
            Write(" Name (press enter for default, 'Player 1'): ");
            string nameInput = ReadLine()!;
            if (nameInput != "") p1Name = nameInput;  //update name if user entered something

            //player 1's symbol
            char p1Symbol = ' ';
            Write(" ONE-CHARACTER symbol (press enter for default, 'X'): ");
            while (p1Symbol == ' ')
            {
                string symbolInput = String.Concat(ReadLine()!.Where(c => !Char.IsWhiteSpace(c)));  //remove any spaces
                if (symbolInput == "") p1Symbol = 'X';
                else if (symbolInput.Length == 1) p1Symbol = symbolInput.ToCharArray()[0];
                else Write(" Symbol must be one character long. Please try again: ");
            }

            //player 1's colour
            ConsoleColor p1Colour = ConsoleColor.White;
            Write(" Colour number ( ");

            //give colour options
            for (int i = 0; i < colours.Count - 1; i++)
            {
                ConsoleColor thisColour = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), colours[i], true);
                WriteInColour(thisColour, "[{0}]{1} ", i + 1, colours[i]);  //offset index by 1
            }

            //last option hit enter
            ConsoleColor lastColour = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), colours[colours.Count-1], true);
            WriteInColour(lastColour, "[ENTER]{0} ): ", colours[colours.Count-1]);

            //prompt user to enter colour choice
            while (p1Colour == ConsoleColor.White)
            {
                string colourInput = String.Concat(ReadLine()!.Where(c => !Char.IsWhiteSpace(c)));  //remove any spaces
                int colourIndex = -1;
                if (colourInput == "") colourIndex = colours.Count-1;  //default press enter
                else if (int.TryParse(colourInput, out colourIndex))
                {
                    //offset index by 1
                    colourIndex--;
                    if (colourIndex < 0 || colourIndex > colours.Count-2)  //selected number is not an option in list
                    {
                        Write(" Not a valid colour option. Please try again: ");
                        continue;
                    }
                }
                else  //not a number entered
                {
                    Write(" Selection must be a number. Please try again: ");
                    continue;
                }

                //update player colour and remove from list
                p1Colour = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), colours[colourIndex], true);
                colours.RemoveAt(colourIndex);
            }

            //player 2
            WriteLine();
            WriteLine(" PLAYER 2");
            WriteLine(" --------");

            //player 2's name
            string p2Name = p1Name;  //set equal for now to check for duplicate
            Write(" Name (press enter for default, 'Player 2'): ");
            while (p2Name == p1Name)
            {
                nameInput = ReadLine()!;
                if (nameInput == "") p2Name = "Player 2";
                else if (nameInput != p1Name) p2Name = nameInput;
                else Write(" Please choose a different name from Player 1: ");
            }

            //player 2's symbol
            char p2Symbol = ' ';
            Write(" ONE-CHARACTER symbol (press enter for default, 'O'): ");
            while (p2Symbol == ' ')
            {
                string symbolInput = String.Concat(ReadLine()!.Where(c => !Char.IsWhiteSpace(c)));  //remove any spaces
                if (symbolInput == "") p2Symbol = 'O';
                else if (symbolInput.Length == 1)
                {
                    p2Symbol = symbolInput.ToCharArray()[0];
                    if (p2Symbol == p1Symbol)  //cannot match player 1's symbol
                    {
                        Write(" Please choose a different symbol from Player 1: ");
                        p2Symbol = ' ';  //reset as empty
                    }
                }
                else Write(" Symbol must be one character long. Please try again: ");
            }

            //player 2's colour
            ConsoleColor p2Colour = ConsoleColor.White;
            Write(" Colour number ( ");

            //give colour options
            for (int i = 0; i < colours.Count - 1; i++)
            {
                ConsoleColor thisColour = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), colours[i], true);
                WriteInColour(thisColour, "[{0}]{1} ", i + 1, colours[i]);  //offset index by 1
            }

            //last option hit enter
            lastColour = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), colours[colours.Count - 1], true);
            WriteInColour(lastColour, "[ENTER]{0} ): ", colours[colours.Count - 1]);

            //prompt user to enter colour choice
            while (p2Colour == ConsoleColor.White)
            {
                string colourInput = String.Concat(ReadLine()!.Where(c => !Char.IsWhiteSpace(c)));  //remove any spaces
                int colourIndex = -1;
                if (colourInput == "") colourIndex = colours.Count - 1;  //default press enter
                else if (int.TryParse(colourInput, out colourIndex))
                {
                    //offset index by 1
                    colourIndex--;
                    if (colourIndex < 0 || colourIndex > colours.Count - 2)  //selected number is not an option in list
                    {
                        Write(" Not a valid colour option. Please try again: ");
                        continue;
                    }
                }
                else  //not a number entered
                {
                    Write(" Selection must be a number. Please try again: ");
                    continue;
                }

                //update player colour
                p2Colour = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), colours[colourIndex], true);

                //check if same as player 1
                if (p2Colour == p1Colour)
                {
                    Write(" Please choose a different colour from player 1: ");
                    p2Colour = ConsoleColor.White;
                }
            }

            //create player objects (discs are created as well)
            Player player1 = new Player(p1Name, p1Symbol, p1Colour);
            Player player2 = new Player(p2Name, p2Symbol, p2Colour);

            return (player1, player2);
        }

        //collect desired board size
        static (int rows, int cols) GetBoardSize()
        {
            WriteLine();
            WriteLine(" BOARD SIZE");
            WriteLine(" ----------");

            int desiredRows = 0;
            int desiredCols = 0;

            Write(" Choose desired number of rows (must be even and between 4 & 26, inclusive). Press enter for default, 8: ");
            while (desiredRows == 0)
            {
                string rowInput = String.Concat(ReadLine()!.Where(c => !Char.IsWhiteSpace(c)));  //remove white space
                if (rowInput == "")  //default
                {
                    desiredRows = 8;
                    break;
                }
                try  //convert string input to integer
                {
                    desiredRows = int.Parse(rowInput);
                    if (desiredRows < 4 || desiredRows > 26 || desiredRows % 2 == 1)  //invalid number
                    {
                        Write(" Must be even and between 4 & 26, inclusive. Please try again: ");
                        desiredRows = 0;
                    }
                }
                catch (FormatException e) //if user enters a non-integer
                {
                    Write(" " + e.Message + " Please try again: ");
                }
            }
            Write(" Choose desired number of columns (must be even and between 4 & 26, inclusive). Press enter for default, 8: ");
            while (desiredCols == 0)
            {
                string colInput = String.Concat(ReadLine()!.Where(c => !Char.IsWhiteSpace(c)));  //remove white space
                if (colInput == "")  //default
                {
                    desiredCols = 8;
                    break;
                }
                try  //convert string input to integer
                {
                    desiredCols = int.Parse(colInput);
                    if (desiredCols < 4 || desiredCols > 26 || desiredCols % 2 == 1)
                    {
                        Write(" Must be even and between 4 & 26, inclusive. Please try again: ");
                        desiredCols = 0;
                    }
                }
                catch (FormatException e) //if user enters a non-integer
                {
                    Write(" " + e.Message + " Please try again: ");
                }
            }

            return (desiredRows, desiredCols);
        }

        //draw scoreboard
        static void Scoreboard(Player p1, Player p2)
        {
            WriteLine();
            WriteLine(" SCOREBOARD");
            WriteLine(" ----------");
            WriteLineInColour(p1.Colour, " {0} ({1}): {2}", p1.Name, p1.Symbol, p1.Score);
            WriteLineInColour(p2.Colour, " {0} ({1}): {2}", p2.Name, p2.Symbol, p2.Score);
            WriteLine();
        }

        //show instructions
        static void Instructions()
        {
            WriteLine();
            WriteLine(" Instructions:");
            WriteLine(" - Enter row + column to place a piece (eg. 'ab').");
            WriteLine(" - Enter 'pass' to pass your turn.");
            WriteLine(" - Enter 'quit' to end game.");
            WriteLine();
        }

        //display winner
        static void GameOver(Player p1, Player p2)
        {
            WriteLine();
            WriteLine(" GAME OVER.");
            WriteLine();

            //tie
            if (p1.Score == p2.Score)
            {
                WriteInColour(p1.Colour, " {0}", p1.Name);
                Write(" and ");
                WriteInColour(p2.Colour, "{0}", p2.Name);
                WriteLine(" tied with {0} points. Good game!", p1.Score);
            }

            //not a tie
            else
            {
                Player winner = p1;
                if (p2.Score > p1.Score) winner = p2;
                int difference = Math.Abs(p1.Score - p2.Score);
                WriteInColour(winner.Colour, " {0}", winner.Name);
                WriteLine(" won by {0} points. Congratulations!", difference);
            }
            WriteLine();
            Write(" Press enter to play again: ");
            if (ReadLine() == "") Main();
            WriteLine();
        }

        //Console.Write() in a specified colour
        public static void WriteInColour(ConsoleColor colour, string text, params object[] objects)
        {
            ForegroundColor = colour;  //change text colour
            Write(text, objects);
            ResetColor();  //reset to white
        }

        //Console.WriteLine() in a specified colour
        public static void WriteLineInColour(ConsoleColor colour, string text, params object[] objects)
        {
            ForegroundColor = colour;  //change text colour
            WriteLine(text, objects);
            ResetColor();  //reset to white
        }

        //converts index (0-25) to letter ('a'-'z')
        public static char IndexToLetter(int index)
        {
            if (index < 0 || index > 25) return ' ';
            return "abcdefghijklmnopqrstuvwxyz"[index];
        }

        //converts letter ('a'-'z') to index (0-25)
        public static int LetterToIndex(char letter)
        {
            return "abcdefghijklmnopqrstuvwxyz".IndexOf(letter);
        }
    }
}

//end of program

