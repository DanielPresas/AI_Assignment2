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
}
