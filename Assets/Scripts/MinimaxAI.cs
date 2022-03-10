public class MinimaxAI {
    public static int FindBestMove(Board currentBoard) {
        var board = currentBoard;
        var bestScore = int.MinValue;
        var move = -1;

        for(int i = 0; i < board.height; ++i) {
            for(int j = 0; j < board.width; ++j) {
                var idx = i * board.width + j;
                if(board.state[idx] == Board.State.Empty) {
                    board.state[idx] = Board.State.AiPiece;
                    var score = Minimax(board, 0, isMaximizing: false);
                    board.state[idx] = Board.State.Empty;

                    if(score > bestScore) {
                        bestScore = score;
                        move = idx;
                    }
                }
            }
        }

        return move;
    }

    static int Minimax(Board board, int depth, bool isMaximizing) {
        if(board.CheckWin(Turn.Ai))     { return  10; }
        if(board.CheckWin(Turn.Player)) { return -10; }
        if(board.CheckTie())            { return   0; }

        if(isMaximizing) {
            var maxScore = int.MinValue;
            for(int i = 0; i < board.height; ++i) {
                for(int j = 0; j < board.width; ++j) {
                    var idx = i * board.width + j;
                    if(board.state[idx] != Board.State.Empty) {
                        continue;
                    }

                    board.state[idx] = Board.State.AiPiece;
                    var score = Minimax(board, depth + 1, isMaximizing: false);
                    board.state[idx] = Board.State.Empty;
                    maxScore = score > maxScore ? score : maxScore;  // @Note: bestScore = max(score, bestScore);
                }
            }

            return maxScore;
        }

        // @Note: if(!isMaximizing)
        var minScore = int.MaxValue;
        for(int i = 0; i < board.height; ++i) {
            for(int j = 0; j < board.width; ++j) {
                var idx = i * board.width + j;
                if(board.state[idx] != Board.State.Empty) {
                    continue;
                }

                board.state[idx] = Board.State.PlayerPiece;
                var score = Minimax(board, depth + 1, isMaximizing: true);
                board.state[idx] = Board.State.Empty;
                minScore = score < minScore ? score : minScore;  // @Note: bestScore = min(score, bestScore);
            }
        }

        return minScore;
    }
}
