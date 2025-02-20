//-----------------------------------------------------------------------
// <copyright file="Chess960Rulebook.cs">
//     Copyright (c) Michael Szvetits. All rights reserved.
// </copyright>
// <author>Michael Szvetits</author>
//-----------------------------------------------------------------------
namespace Chess.Model.Rule
{
    using Chess.Model.Command;
    using Chess.Model.Data;
    using Chess.Model.Game;
    using Chess.Model.Piece;
    using Chess.Model.Visitor;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents the Chess960 chess rulebook.
    /// </summary>
    public class Chess960Rulebook : IRulebook
    {
        /// <summary>
        /// Represents the check rule of a Chess960 chess game.
        /// </summary>
        private readonly CheckRule checkRule;

        /// <summary>
        /// Represents the end rule of a Chess960 chess game.
        /// </summary>
        private readonly EndRule endRule;

        /// <summary>
        /// Represents the movement rule of a Chess960 chess game.
        /// </summary>
        private readonly MovementRule movementRule;

        /// <summary>
        /// Initializes a new instance of the <see cref="Chess960Rulebook"/> class.
        /// </summary>
        public Chess960Rulebook()
        {
            var threatAnalyzer = new ThreatAnalyzer();
            var castlingRule = new CastlingRule(threatAnalyzer);
            var enPassantRule = new EnPassantRule();
            var promotionRule = new PromotionRule();

            this.checkRule = new CheckRule(threatAnalyzer);
            this.movementRule = new MovementRule(castlingRule, enPassantRule, promotionRule, threatAnalyzer);
            this.endRule = new EndRule(this.checkRule, this.movementRule);
        }

        /// <summary>
        /// Creates a new chess game according to the standard rulebook.
        /// </summary>
        /// <returns>The newly created chess game.</returns>
        public ChessGame CreateGame()
        {
            IEnumerable<PlacedPiece> makeBaseLine(int row, Color color)
            {
                //Researched the Fisher-Yates Shuffle alrorithim online
                //Structure of the algorithm from online

                //Create random class
                var rnd = new Random();

                //Create list of pieces in their orginal potitions
                var pieces = new ChessPiece[8]
                {
                    new Rook(color),
                    new Knight(color),
                    new Bishop(color),
                    new Queen(color),
                    new King(color),
                    new Bishop(color),
                    new Knight(color),
                    new Rook(color)
                };

                //Bool to keep loop running until valid configuration
                bool valid = false;

                //Loop through the pieces array in reverse order
                for (int i = pieces.Length - 1; i > 0; i--)
                {
                    while(!valid)
                    {
                        //Random index from 0 to i
                        int j = rnd.Next(0, i + 1);

                        //Swap positions between random pieces
                        (pieces[i], pieces[j]) = (pieces[j], pieces[i]);

                        //Get the important pieces positions - Found thesen methods online
                        var bishopPositions = Enumerable.Range(0, 8).Where(ind => pieces[ind] is Bishop).ToList();
                        var rookPositions = Enumerable.Range(0, 8).Where(ind => pieces[ind] is Rook).ToList();
                        var kingPosition = Array.FindIndex(pieces, p => p is King);

                        //Check if Bishops are on opposite colors using mod
                        bool checkB = CheckBishops(bishopPositions[0], bishopPositions[1]);

                        //Check if King is between rooks
                        bool checkK = CheckKingAndRooks(rookPositions[0], rookPositions[1], kingPosition);

                        //Break the loop when there is a valid configuration
                        valid = checkB && checkK;
                    }

                }

                // Return pieces in a valid format that works with 
                return Enumerable.Range(0, 8).Select(i => new PlacedPiece(new Position(row, i), pieces[i]));
            }

            IEnumerable<PlacedPiece> makePawns(int row, Color color) =>
                Enumerable.Range(0, 8).Select(
                    i => new PlacedPiece(new Position(row, i), new Pawn(color))
                );

            IImmutableDictionary<Position, ChessPiece> makePieces(int pawnRow, int baseRow, Color color)
            {
                var pawns = makePawns(pawnRow, color);
                var baseLine = makeBaseLine(baseRow, color);
                var pieces = baseLine.Union(pawns);
                var empty = ImmutableSortedDictionary.Create<Position, ChessPiece>(PositionComparer.DefaultComparer);
                return pieces.Aggregate(empty, (s, p) => s.Add(p.Position, p.Piece));
            }

            var whitePlayer = new Player(Color.White);
            var whitePieces = makePieces(1, 0, Color.White);
            var blackPlayer = new Player(Color.Black);
            var blackPieces = makePieces(6, 7, Color.Black);
            var board = new Board(whitePieces.AddRange(blackPieces));

            return (CheckGameBoard(board)) ? new ChessGame(board, whitePlayer, blackPlayer) : null;
        }

        /// <summary>
        /// Method to check Bishop configuration
        /// </summary>
        /// <param name="Bish1">Postion in array of Bishop 1</param>
        /// <param name="Bish2">Postion in array of Bishop 2</param>
        /// <returns></returns>
        public bool CheckBishops(int Bish1, int Bish2)
        {
            return Bish1 % 2 != Bish2 % 2;
        }

        /// <summary>
        /// Method to check King and Rook configuration
        /// </summary>
        /// <param name="Rook1">Postion in array of Rook 1</param>
        /// <param name="Rook2">Postion in array of Rook 2</param>
        /// <param name="King">Postion in array of King</param>
        /// <returns></returns>
        public bool CheckKingAndRooks(int Rook1, int Rook2, int King)
        {
            return King > Rook1 && King < Rook2;
        }

        /// <summary>
        /// Check if the board created the correct number of black and white pieces
        /// </summary>
        /// <param name="bd">The board to be checked</param>
        /// <returns>True or False depending on the number of pieces</returns>
        public bool CheckGameBoard(Board bd)
        {
            var blackPieces = 0;
            var whitePieces = 0;
            foreach (var item in bd)
            {
                if(item.Color == Color.Black) blackPieces++;
                else if(item.Color == Color.White) whitePieces++;
            }

            if(blackPieces == 16 && whitePieces == 16) return true;
            else return false;
        }

        /// <summary>
        /// Gets the status of a chess game, according to the standard rulebook.
        /// </summary>
        /// <param name="game">The game state to be analyzed.</param>
        /// <returns>The current status of the game.</returns>
        public Status GetStatus(ChessGame game)
        {
            return this.endRule.GetStatus(game);
        }

        /// <summary>
        /// Gets all possible updates (i.e., future game states) for a chess piece on a specified position,
        /// according to the standard rulebook.
        /// </summary>
        /// <param name="game">The current game state.</param>
        /// <param name="position">The position to be analyzed.</param>
        /// <returns>A sequence of all possible updates for a chess piece on the specified position.</returns>
        public IEnumerable<Update> GetUpdates(ChessGame game, Position position)
        {
            var piece = game.Board.GetPiece(position, game.ActivePlayer.Color);
            var updates = piece.Map(
                p =>
                {
                    var moves = this.movementRule.GetCommands(game, p);
                    var turnEnds = moves.Select(c => new SequenceCommand(c, EndTurnCommand.Instance));
                    var records = turnEnds.Select
                    (
                        c => new SequenceCommand(c, new SetLastUpdateCommand(new Update(game, c)))
                    );
                    var futures = records.Select(c => c.Execute(game).Map(g => new Update(g, c)));
                    return futures.FilterMaybes().Where
                    (
                        e => !this.checkRule.Check(e.Game, e.Game.PassivePlayer)
                    );
                }
            );

            return updates.GetOrElse(Enumerable.Empty<Update>());
        }
    }
}