using UnityEngine;

public class Board : MonoBehaviour {
    public const int WIDTH  = 3;
    public const int HEIGHT = 3;

    public enum State {
        Empty,
        NewPlayerPiece,
        NewAiPiece,
        PlayerPiece,
        AiPiece,
    }

    public GameObject oPiece  = null;
    public GameObject xPiece  = null;
    public GameObject trigger = null;
    public int squareSize     = 5;

    public State[] state = new State[WIDTH * HEIGHT] {
        State.Empty, State.Empty, State.Empty,
        State.Empty, State.Empty, State.Empty,
        State.Empty, State.Empty, State.Empty,
    };

    public bool CheckWin(GameManager.Turn turnToCheck) {
        bool CheckRow(int row) {
            var st = state[row * HEIGHT];
            var check = true;

            for(int j = 1; j < WIDTH; ++j) {
                if(state[row * HEIGHT + j] != st) {
                    check = false;
                    break;
                }
            }

            return check && st != State.Empty;
        }

        bool CheckColumn(int col) {
            var st = state[col];
            var check = true;

            for(int i = 1; i < HEIGHT; ++i) {
                if(state[i * HEIGHT + col] != st) {
                    check = false;
                    break;
                }
            }

            return check && st != State.Empty;
        }

        bool CheckDiagonal(bool upwards) {
            var check = true;
            if(upwards) {
                var st = state[0];
                for(int i = 1; i < HEIGHT; ++i) {
                    if(state[i * HEIGHT + i] != st) {
                        check = false;
                        break;
                    }
                }
                return check && st != State.Empty;
            }
            else {
                var st = state[WIDTH - 1];
                for(
                    int i = 0, j = WIDTH - 1;
                    i < HEIGHT && j >= 0;
                    ++i, --j
                ) {
                    if(state[i * HEIGHT + j] != st) {
                        check = false;
                        break;
                    }
                }
                return check && st != State.Empty;
            }

        }

        // @Note: Horizontal check
        for(int i = 0; i < HEIGHT; ++i) {
            var row = i * HEIGHT;
            if(!CheckRow(i)) {
                continue;
            }

            if(state[row] == State.PlayerPiece && turnToCheck == GameManager.Turn.Player) {
                return true;
            }
            else if(state[row] == State.AiPiece && turnToCheck == GameManager.Turn.AI) {
                return true;
            }
        }

        // @Note: Vertical check
        for(int j = 0; j < WIDTH; ++j) {
            if(!CheckColumn(j)) {
                continue;
            }

            if(state[j] == State.PlayerPiece && turnToCheck == GameManager.Turn.Player) {
                return true;
            }
            else if(state[j] == State.AiPiece && turnToCheck == GameManager.Turn.AI) {
                return true;
            }
        }

        // @Note: Diagonal checks
        if(CheckDiagonal(upwards: true)) {
            if(state[0] == State.PlayerPiece && turnToCheck == GameManager.Turn.Player) {
                return true;
            }
            else if(state[0] == State.AiPiece && turnToCheck == GameManager.Turn.AI) {
                return true;
            }
        }

        if(CheckDiagonal(upwards: false)) {
            if(state[WIDTH - 1] == State.PlayerPiece && turnToCheck == GameManager.Turn.Player) {
                return true;
            }
            else if(state[WIDTH - 1] == State.AiPiece && turnToCheck == GameManager.Turn.AI) {
                return true;
            }
        }

        return false;
    }
}
