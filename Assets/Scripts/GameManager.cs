using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public Board gameBoard = null;
    public bool playerTurn = true;
    public bool stateChanged = false;

    List<GameObject> _playerPieces = new List<GameObject>();
    List<GameObject> _aiPieces = new List<GameObject>();

    int   _activePlayerPieces = 0;
    int   _activeAiPieces     = 0;

    Coroutine _resetCoroutine = null;

    private void Start() {
        _resetCoroutine = null;

        for(int i = 0; i < 5; ++i) {
            var pp = Instantiate(gameBoard.xPiece, gameBoard.transform);
            pp.SetActive(false);
            _playerPieces.Add(pp);
        }
        for(int i = 0; i < 4; ++i) {
            var ap = Instantiate(gameBoard.oPiece, gameBoard.transform);
            ap.SetActive(false);
            _aiPieces.Add(ap);
        }

        Debug.Log("Player Turn");
    }


    private void Update() {
        if(_activePlayerPieces + _activeAiPieces == gameBoard.state.Length) {
            if(_resetCoroutine == null) {
                _resetCoroutine = StartCoroutine(ResetLevel());
            }
            return;
        }

        if(playerTurn) {
            if(Input.GetKeyDown(KeyCode.S)) {
                gameBoard.state[_activePlayerPieces] = Board.State.NewPlayerPiece;
                stateChanged = true;
                playerTurn = false;

                Debug.Log("AI Turn");
            }
        }
        else {
            if(Input.GetKeyDown(KeyCode.A)) {
                gameBoard.state[8 - _activeAiPieces] = Board.State.NewAiPiece;
                stateChanged = true;
                playerTurn = true;

                Debug.Log("Player Turn");
            }
        }

        if(!stateChanged) {
            return;
        }

        for(int i = 0; i < gameBoard.state.Length; ++i) {
            var s = gameBoard.state[i];
            switch(s) {
                case Board.State.NewPlayerPiece:
                    var pp = _playerPieces[_activePlayerPieces++];
                    pp.SetActive(true);
                    pp.transform.position = new Vector3 {
                        x = 0.0f,
                        y = 0.2f * i,
                        z = 0.0f,
                    };
                    gameBoard.state[i] = Board.State.PlayerPiece;
                    break;
                case Board.State.NewAiPiece:
                    var ap = _aiPieces[_activeAiPieces++];
                    ap.SetActive(true);
                    ap.transform.position = new Vector3 {
                        x = 10.0f,
                        y = 0.2f * i,
                        z = 10.0f
                    };
                    gameBoard.state[i] = Board.State.AiPiece;
                    break;

                default: break;
            }
        }

        stateChanged = false;
    }

    IEnumerator ResetLevel() {
        Debug.Log("Draw! Resetting...");
        yield return new WaitForSeconds(5.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
