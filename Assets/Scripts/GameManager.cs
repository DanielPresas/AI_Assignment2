using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MouseButton {
    public const int Left = 0;
    public const int Right = 1;
    public const int Middle = 2;
}

public class GameManager : MonoBehaviour {
    public Board gameBoard = null;
    public bool playerTurn = true;
    public bool stateChanged = false;

    List<GameObject> _playerPieces = new List<GameObject>();
    List<GameObject> _aiPieces = new List<GameObject>();
    // List<GameObject> _triggers = new List<GameObject>();

    int _activePlayerPieces = 0;
    int _activeAiPieces = 0;
    int _activeTriggers = 0;

    Coroutine _resetCoroutine = null;

    private void Start() {
        _resetCoroutine = null;

        for(int i = 0; i < Mathf.CeilToInt(Board.WIDTH * Board.HEIGHT / 2.0f); ++i) {
            var pp = Instantiate(gameBoard.xPiece, gameBoard.transform);
            pp.SetActive(false);
            _playerPieces.Add(pp);
        }
        for(int i = 0; i < Mathf.FloorToInt(Board.WIDTH * Board.HEIGHT / 2.0f); ++i) {
            var ap = Instantiate(gameBoard.oPiece, gameBoard.transform);
            ap.SetActive(false);
            _aiPieces.Add(ap);
        }

        for(int i = 0; i < Board.HEIGHT; ++i) {
            for(int j = 0; j < Board.WIDTH; ++j) {
                var t = Instantiate(gameBoard.trigger, gameBoard.transform);
                t.transform.position = new Vector3 {
                    x = j * gameBoard.squareSize,
                    z = i * gameBoard.squareSize,
                };
                t.GetComponent<Trigger>().index = _activeTriggers;

                // _triggers.Add(t);
                _activeTriggers += 1;
                t.name = $"Trigger_{_activeTriggers}";
            }
        }

        Debug.Log("Player Turn");
    }


    private void Update() {
        // @Refactor:
        // if(_activeTriggers <= 0) {
        if(_activePlayerPieces + _activeAiPieces == gameBoard.state.Length) {
            if(_resetCoroutine == null) {
                _resetCoroutine = StartCoroutine(ResetLevel());
            }
            return;
        }

        if(playerTurn) {
            if(Input.GetMouseButtonDown(MouseButton.Left)) {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(!Physics.Raycast(ray, out var hitInfo, maxDistance: 20.0f)) {
                    Debug.DrawRay(ray.origin, ray.direction * 20.0f, Color.red, duration: 5.0f);
                    return;
                }

                Debug.Log($"Hit: {hitInfo.collider.gameObject.name}");
                var t = hitInfo.collider.gameObject.GetComponent<Trigger>();
                if(gameBoard.state[t.index] != Board.State.Empty) {
                    Debug.DrawRay(ray.origin, ray.direction * 20.0f, Color.red, duration: 5.0f);
                    return;
                }

                Debug.DrawRay(ray.origin, ray.direction * 20.0f, Color.green, duration: 5.0f);
                gameBoard.state[t.index] = Board.State.NewPlayerPiece;
                t.gameObject.SetActive(false);
                _activeTriggers -= 1;
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

        for(int i = 0; i < Board.HEIGHT; ++i) {
            for(int j = 0; j < Board.WIDTH; ++j) {
                var idx = i * Board.HEIGHT + j;
                var s = gameBoard.state[idx];
                switch(s) {
                    case Board.State.NewPlayerPiece:
                        var pp = _playerPieces[_activePlayerPieces++];
                        pp.SetActive(true);
                        pp.transform.position = new Vector3 {
                            x = j * gameBoard.squareSize,
                            y = 0.2f,
                            z = i * gameBoard.squareSize,
                        };
                        gameBoard.state[idx] = Board.State.PlayerPiece;
                        break;
                    case Board.State.NewAiPiece:
                        var ap = _aiPieces[_activeAiPieces++];
                        ap.SetActive(true);
                        ap.transform.position = new Vector3 {
                            x = j * gameBoard.squareSize,
                            y = 0.2f,
                            z = i * gameBoard.squareSize,
                        };
                        gameBoard.state[idx] = Board.State.AiPiece;
                        break;

                    default: break;
                }
            }

            stateChanged = false;
        }
    }

    IEnumerator ResetLevel() {
        Debug.Log("Draw! Resetting...");
        yield return new WaitForSeconds(5.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
