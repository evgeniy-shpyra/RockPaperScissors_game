using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleTables;
using System.Security.Cryptography;




namespace game1
{

    namespace game
    {
        class Program
        {
            static void Main(string[] args)
            {
                Dictionary<int, string> Moves = EnteringMoves();
                Rules rulesOfTheGame = new Rules(Moves.Count());
                HelpMenu helpMenu = new HelpMenu(rulesOfTheGame.GetRulesOfMoves(), Moves);
                Key key = new Key(ref Moves);

                string inputStr;
                int userMove;
                int psMove = key.GetPcMove();
                int gameResult;

                do
                {
                    PrintMenu(ref Moves, key.GetHmac());

                    inputStr = Console.ReadLine();

                    if (inputStr == "?")
                        helpMenu.DrowTable();
                    ;

                } while (!int.TryParse(inputStr, out userMove) || (userMove < 0 || userMove > Moves.Count()));

                if (userMove == 0)
                    return;

                Console.WriteLine("Computer move: " + Moves[key.GetPcMove()]);

                psMove = 4;
                gameResult = rulesOfTheGame.DeterminingTheWinner(userMove, psMove);

                if (gameResult == 1)
                    Console.WriteLine("You win!");
                else if (gameResult == -1)
                    Console.WriteLine("You lose!");
                else
                    Console.WriteLine("Draw!");

                Console.WriteLine("HMAC key: " + key.GetKey());

                return;
            }
            static Dictionary<int, string> EnteringMoves()
            {
                string[] allMoves;
                do
                {
                    string moves = Console.ReadLine();
                    allMoves = moves.Split(' ');
                    if ((allMoves.Length >= 3 && allMoves.Length % 2 != 0) && (allMoves.Distinct().Count() == allMoves.Length))
                        break;
                    Console.Clear();
                    Console.WriteLine("Input Error\nInput should be for example: rock paper scissors lizard Spock\n");
                } while (true);

         

                Dictionary<int, string> Moves = new Dictionary<int, string>();



                for (int i = 0; i < allMoves.Length; i++)
                    Moves.Add(i + 1, allMoves[i]);

                return Moves;
            }
            static void PrintMenu(ref Dictionary<int, string> Moves, string hmac)
            {
                Console.Clear();
                Console.WriteLine("HMAC:");
                Console.WriteLine(hmac);
                Console.WriteLine("Available moves:");

                for (int i = 1; i <= Moves.Count(); i++)
                {
                    Console.Write(i + " - " + Moves[i] + "\n");
                }

                Console.Write("0 - exit" +
                    "\n? - help" +
                    "\nEnter your move: ");
            }
        }

        class HelpMenu
        {
            public HelpMenu(int[,] rulesOfMoves, Dictionary<int, string> Moves)
            {
                DataInTable = new string[Moves.Count + 1][];

                for (int i = 0; i < Moves.Count + 1; i++)
                    DataInTable[i] = new string[Moves.Count + 1];

                PopulatingTable(ref rulesOfMoves, ref Moves);
            }

            private void PopulatingTable(ref int[,] rulesOfMoves, ref Dictionary<int, string> Moves)
            {

                DataInTable[0][0] = "PC / Uswer";
                for (int i = 1; i < DataInTable.GetLength(0); i++)
                    DataInTable[0][i] = Moves[i];

                table = new ConsoleTable(DataInTable[0]);

                for (int i = 1; i < DataInTable.GetLength(0); i++)
                {
                    DataInTable[i][0] = Moves[i];

                    for (int j = 1; j < DataInTable.GetLength(0); j++)
                    {
                        DataInTable[i][j] = rulesOfMoves[i - 1, j - 1] == 1 ? "LOSE" : "WIN";
                        DataInTable[j][j] = "DROW";
                    }
                    table.AddRow(DataInTable[i]);
                }
            }

            public void DrowTable()
            {
                Console.WriteLine();
                table.Write(Format.Alternative);
                Console.Read();
            }



            private string[][] DataInTable;

            private ConsoleTable table;
        }

        class Rules
        {
            public Rules(int NumberOfMoves)
            {
                FillingInConditions(NumberOfMoves);
            }

            private void FillingInConditions(int NumberOfMoves)
            {
                rulesOfMoves = new int[NumberOfMoves, NumberOfMoves];
                int step = NumberOfMoves / 2;
                for (int i = 0; i < rulesOfMoves.GetLength(0); i++)
                {
                    for (int j = i + 1; j < rulesOfMoves.GetLength(0); j++)
                    {
                        if (j <= i + step)
                        {
                            rulesOfMoves[i, j] = -1;
                            rulesOfMoves[j, i] = -rulesOfMoves[i, j];
                        }
                        else
                        {
                            rulesOfMoves[i, j] = 1;
                            rulesOfMoves[j, i] = -rulesOfMoves[i, j];
                        }
                    }
                }
            }

            public int DeterminingTheWinner(int moveOfFirstPlayer, int moveOfSecondPlayer)
            {
                moveOfFirstPlayer--;
                moveOfSecondPlayer--;
                return rulesOfMoves[moveOfFirstPlayer, moveOfSecondPlayer];
            }

            public int[,] GetRulesOfMoves()
            {
                return rulesOfMoves;
            }



            private int[,] rulesOfMoves;
        }

        class Key
        {
            public Key(ref Dictionary<int, string> Moves)
            {
                key = new byte[keyLenght / 8];

                sha256 = SHA256.Create();
                numberGen = RandomNumberGenerator.Create();

                numberGen.GetBytes(key);
                pcMove = RandomNumberGenerator.GetInt32(1, Moves.Count);

                string pcMoveStr = Moves[pcMove];
                pcMoveByte = System.Text.Encoding.UTF8.GetBytes(pcMoveStr); 
            }

            public string GetKey()
            {
                return String.Concat<byte>(key);   
            }

            public int GetPcMove()
            {
                return pcMove;
            }

            public string GetHmac()
            {
                byte[] keyAndPcMove = new byte[key.Length + pcMoveByte.Length];

                Array.Copy(key, 0, keyAndPcMove, 0, key.Length);
                Array.Copy(pcMoveByte, 0, keyAndPcMove, key.Length, pcMoveByte.Length);

                var hmacKey = sha256.ComputeHash(keyAndPcMove);
                     
                return String.Concat<byte>(hmacKey);
            }


            private byte[] key;

            private byte[] pcMoveByte;

            private int pcMove;

            private RandomNumberGenerator numberGen;

            private SHA256 sha256;

            private const int keyLenght = 128;
        }
    }
}
