using Microsoft.VisualStudio.TestTools.UnitTesting;
using Chess.ViewModel.Game;
using Chess.Model.Piece;
using Chess.Model.Rule;
using System.Linq;
using System;
using Chess.Model.Game;

namespace UnitTests
{
    [TestClass]
    public class ChessGame
    {
        [TestMethod]
        public void KingBetweenRooks_HappyPath()
        {
            var color = Color.White;
            var pieces = new ChessPiece[8]
            {
                    new Rook(color),
                    new Knight(color),
                    new King(color),
                    new Bishop(color),
                    new Rook(color),
                    new Queen(color),
                    new Bishop(color),
                    new Knight(color)
            };

            var chessRB = new Chess960Rulebook();

            var rookPositions = Enumerable.Range(0, 8).Where(ind => pieces[ind] is Rook).ToList();
            var kingPosition = Array.FindIndex(pieces, p => p is King);

            bool check = chessRB.CheckKingAndRooks(rookPositions[0], rookPositions[1], kingPosition);

            Assert.AreEqual(true, check);
        }

        [TestMethod]
        public void KingBetweenRooks_SadPath()
        {
            var color = Color.White;
            var pieces = new ChessPiece[8]
            {
                    new King(color),
                    new Rook(color),
                    new Knight(color),
                    new Bishop(color),
                    new Queen(color),
                    new Bishop(color),
                    new Rook(color),
                    new Knight(color)
            };

            var chessRB = new Chess960Rulebook();

            var rookPositions = Enumerable.Range(0, 8).Where(ind => pieces[ind] is Rook).ToList();
            var kingPosition = Array.FindIndex(pieces, p => p is King);

            bool check = chessRB.CheckKingAndRooks(rookPositions[0], rookPositions[1], kingPosition);

            Assert.AreEqual(false, check);
        }

        [TestMethod]
        public void BishopsAlternatingColors_HappyPath()
        {
            var color = Color.White;
            var pieces = new ChessPiece[8]
            {
                    new Rook(color),
                    new Bishop(color),
                    new Bishop(color),
                    new Knight(color),
                    new King(color),
                    new Rook(color),
                    new Queen(color),
                    new Knight(color)
            };

            var chessRB = new Chess960Rulebook();

            var bishopPositions = Enumerable.Range(0, 8).Where(ind => pieces[ind] is Bishop).ToList();

            bool check = chessRB.CheckBishops(bishopPositions[0], bishopPositions[1]);

            Assert.AreEqual(true, check);
        }

        [TestMethod]
        public void BishopsAlternatingColors_SadPath()
        {
            var color = Color.White;
            var pieces = new ChessPiece[8]
            {
                    new Rook(color),
                    new Bishop(color),
                    new Knight(color),
                    new King(color),
                    new Rook(color),
                    new Bishop(color),
                    new Queen(color),
                    new Knight(color)
            };

            var chessRB = new Chess960Rulebook();

            var bishopPositions = Enumerable.Range(0, 8).Where(ind => pieces[ind] is Bishop).ToList();

            bool check = chessRB.CheckBishops(bishopPositions[0], bishopPositions[1]);

            Assert.AreEqual(false, check);
        }

        [TestMethod]
        public void ValidBoardSetUp_HappyPath()
        {
            var chessRB = new Chess960Rulebook();

            var game = chessRB.CreateGame();

            Assert.IsNotNull(game);
        }
    }
}
