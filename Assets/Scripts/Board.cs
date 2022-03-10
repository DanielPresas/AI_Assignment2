using UnityEngine;

public class Board : MonoBehaviour {
    public enum State {
        Empty,
        NewPlayerPiece,
        NewAiPiece,
        PlayerPiece,
        AiPiece,
    }

    public GameObject oPiece = null;
    public GameObject xPiece = null;
    public int squareSize = 5;

    public State[] state = {
        State.Empty, State.Empty, State.Empty,
        State.Empty, State.Empty, State.Empty,
        State.Empty, State.Empty, State.Empty,
    };
}
