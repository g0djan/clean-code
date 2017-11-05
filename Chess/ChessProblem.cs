using System.Linq;

namespace Chess
{
	public class ChessProblem
	{
		private static Board board;
		public static ChessStatus ChessStatus;

		public static void LoadFrom(string[] lines)
		{
			board = new BoardParser().ParseBoard(lines);
		}

	    enum PredictStatus
	    {
	        
	    }
		// Определяет мат, шах или пат белым.
		public static void CalculateChessStatus()
		{
			var isCheck = IsCheckForWhite(board);
		    var hasMoves = board.GetPieces(PieceColor.White)
		        .Any(whiteLocation => board.GetPiece(whiteLocation).GetMoves(whiteLocation, board)
		            .Any(locTo => HasSafeMoves(whiteLocation, locTo, false)));
			if (isCheck)
				ChessStatus = hasMoves ? ChessStatus.Check : ChessStatus.Mate;
			else if (hasMoves) ChessStatus = ChessStatus.Ok;
			else ChessStatus = ChessStatus.Stalemate;
		}

	    private static bool HasSafeMoves(Location whiteLocation, Location locTo, bool hasMoves)
	    {
	        var tempBoard = new TemporaryPieceMove(board, whiteLocation, locTo, board.GetPiece(whiteLocation));
	        using (tempBoard)
	        {
	            tempBoard.Swap();
	            if (!IsCheckForWhite(tempBoard.board))
	                hasMoves = true;
	        }
	        return hasMoves;
	    }

	    // check — это шах
		private static bool IsCheckForWhite(Board board)
		{
		    return (from blackLocation in board.GetPieces(PieceColor.Black)
		               let blackPiece = board.GetPiece(blackLocation)
		               select blackPiece.GetMoves(blackLocation, board)).Any(moves => moves
		               .Any(destination => board.GetPiece(destination)
		                                        .Is(PieceColor.White, PieceType.King)));
		}
	}
}