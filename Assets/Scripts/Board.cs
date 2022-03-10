using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
    public int width  = 3;
    public int height = 3;

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
    public Vector2 squareSize = new Vector2();

    public List<State> state = new List<State>();

    private void Awake() {
        state.Clear();
        for(int i = 0; i < width * height; ++i) {
            state.Add(State.Empty);
        }

        squareSize = new Vector2 {
            x = GetComponentInChildren<MeshRenderer>().transform.localScale.x / width,
            y = GetComponentInChildren<MeshRenderer>().transform.localScale.x / height,
        };

    }

    public bool CheckTie() {
        foreach(var s in state) {
            if(s == State.Empty) return false;
        }
        return true;
    }

    public bool CheckWin(Turn turnToCheck) {
        bool CheckRow(int row) {
            var st = state[row * width];
            var check = true;

            for(int j = 1; j < width; ++j) {
                if(state[row * width + j] != st) {
                    check = false;
                    break;
                }
            }

            return check && st != State.Empty;
        }

        bool CheckColumn(int col) {
            var st = state[col];
            var check = true;

            for(int i = 1; i < height; ++i) {
                if(state[i * width + col] != st) {
                    check = false;
                    break;
                }
            }

            return check && st != State.Empty;
        }

        bool CheckDiagonal(bool upwards) {
            if(width != height) return false; // @Note: can't check diagonals on a non-square grid

            var check = true;
            if(upwards) {
                var st = state[0];
                for(int i = 1; i < height; ++i) {
                    if(state[i * width + i] != st) {
                        check = false;
                        break;
                    }
                }
                return check && st != State.Empty;
            }
            else {
                var st = state[width - 1];
                for(
                    int i = 0, j = width - 1;
                    i < height && j >= 0;
                    ++i, --j
                ) {
                    if(state[i * width + j] != st) {
                        check = false;
                        break;
                    }
                }
                return check && st != State.Empty;
            }

        }

        // @Note: Horizontal check
        for(int i = 0; i < height; ++i) {
            var row = i * width;
            if(!CheckRow(i)) {
                continue;
            }

            if(state[row] == State.PlayerPiece && turnToCheck == Turn.Player) {
                return true;
            }
            else if(state[row] == State.AiPiece && turnToCheck == Turn.Ai) {
                return true;
            }
        }

        // @Note: Vertical check
        for(int j = 0; j < width; ++j) {
            if(!CheckColumn(j)) {
                continue;
            }

            if(state[j] == State.PlayerPiece && turnToCheck == Turn.Player) {
                return true;
            }
            else if(state[j] == State.AiPiece && turnToCheck == Turn.Ai) {
                return true;
            }
        }

        // @Note: Diagonal checks
        if(CheckDiagonal(upwards: true)) {
            if(state[0] == State.PlayerPiece && turnToCheck == Turn.Player) {
                return true;
            }
            else if(state[0] == State.AiPiece && turnToCheck == Turn.Ai) {
                return true;
            }
        }

        if(CheckDiagonal(upwards: false)) {
            if(state[width - 1] == State.PlayerPiece && turnToCheck == Turn.Player) {
                return true;
            }
            else if(state[width - 1] == State.AiPiece && turnToCheck == Turn.Ai) {
                return true;
            }
        }

        return false;
    }
}
