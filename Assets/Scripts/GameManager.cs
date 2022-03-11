using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MouseButton {
    public const int Left   = 0;
    public const int Right  = 1;
    public const int Middle = 2;
}

public enum Turn {
    Player, Ai
}

public class GameManager : MonoBehaviour {

    [Header("Game Objects")]
    public Board gameBoard   = null;
    public bool stateChanged = false;
    public Turn turn         = Turn.Player;

    [Header("UI Objects")]
    public TextMeshProUGUI currentTurnText = null;
    public TextMeshProUGUI[] winText       = null;
    [Space]
    public Color xColor = new Color(0.37f, 0.54f, 0.38f);
    public Color oColor = new Color(0.81f, 0.54f, 0.30f);

    List<GameObject> _playerPieces = new List<GameObject>();
    List<GameObject> _aiPieces     = new List<GameObject>();
    List<GameObject> _triggers     = new List<GameObject>();

    int _activePlayerPieces = 0;
    int _activeAiPieces     = 0;
    int _activeTriggers     = 0;

    Coroutine _resetCoroutine = null;
    float _aiWaitTime = 0.0f;
    Color _playerColor, _aiColor;

    private void Update() {
        {
            var endGame = false;

            if(gameBoard.CheckWin(Turn.Player)) {
                winText[0].text = "Player wins!";
                endGame = true;
            }
            else if(gameBoard.CheckWin(Turn.Ai)) {
                winText[0].text = "AI wins!";
                endGame = true;
            }
            else if(gameBoard.CheckTie()) {
                winText[0].text = "Draw!";
                endGame = true;
            }

            if(endGame) {
                winText[0].gameObject.SetActive(true);
                winText[1].gameObject.SetActive(true);

                if(_resetCoroutine == null) {
                    _resetCoroutine = StartCoroutine(ResetLevel());
                }
                return;
            }
        }

        currentTurnText.text = $"{(turn == Turn.Ai ? turn.ToString().ToUpper() : turn.ToString())}";

        if(turn == Turn.Player) {
            currentTurnText.color = _playerColor;
            if(Input.GetMouseButtonDown(MouseButton.Left)) {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(!Physics.Raycast(ray, out var hitInfo, maxDistance: 20.0f)) {
                    return;
                }

                var t = hitInfo.collider.gameObject.GetComponent<Trigger>();
                if(gameBoard.state[t.index] != Board.State.Empty) {
                    return;
                }

                gameBoard.state[t.index] = Board.State.NewPlayerPiece;
                t.gameObject.SetActive(false);
                _activeTriggers -= 1;

                stateChanged = true;
                turn = Turn.Ai;
                _aiWaitTime = 0.5f;
            }
        }
        else {
            currentTurnText.color = _aiColor;

            if(_aiWaitTime >= 0.0f) {
                _aiWaitTime -= Time.deltaTime;
                return;
            }

            do {
                var bestMove = MinimaxAI.FindBestMove(gameBoard);
                if(!_triggers[bestMove].activeSelf) continue;

                gameBoard.state[bestMove] = Board.State.NewAiPiece;
                _triggers[bestMove].SetActive(false);
                _activeTriggers -= 1;

                stateChanged = true;
            }
            while(!stateChanged);

            turn = Turn.Player;
        }

        UpdateBoard();
    }

    IEnumerator ResetLevel() {
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResetGame(bool playerIsX = true) {
        _resetCoroutine = null;

        winText[0].gameObject.SetActive(false);
        winText[1].gameObject.SetActive(false);
        currentTurnText.gameObject.SetActive(true);

        for(int i = 0; i < gameBoard.height; ++i) {
            for(int j = 0; j < gameBoard.width; ++j) {
                var t = Instantiate(gameBoard.trigger, gameBoard.transform);
                t.transform.position = new Vector3 {
                    x = j * gameBoard.squareSize.x,
                    z = i * gameBoard.squareSize.y,
                };
                t.GetComponent<Trigger>().index = _activeTriggers;
                t.GetComponent<BoxCollider>().size = new Vector3 {
                    x = gameBoard.squareSize.x,
                    y = 1.0f,
                    z = gameBoard.squareSize.y,
                };
                t.GetComponent<BoxCollider>().center = new Vector3 {
                    x = gameBoard.squareSize.x / 2.0f,
                    y = 0.0f,
                    z = gameBoard.squareSize.y / 2.0f,
                };

                _triggers.Add(t);
                _activeTriggers += 1;
                t.name = $"Trigger_{_activeTriggers}";
            }
        }

        var xPieces = new List<GameObject>();
        for(int i = 0; i < Mathf.CeilToInt(gameBoard.width * gameBoard.height / 2.0f); ++i) {
            var xp = Instantiate(gameBoard.xPiece, gameBoard.transform);

            var xpm = xp.GetComponentInChildren<MeshRenderer>();
            xpm.material.color = xColor;
            xpm.transform.localScale = new Vector3 {
                x = gameBoard.squareSize.x * 0.8f,
                y = 0.2f,
                z = gameBoard.squareSize.y * 0.8f,
            };
            xpm.transform.position = new Vector3 {
                x = gameBoard.squareSize.x / 2.0f,
                z = gameBoard.squareSize.y / 2.0f,
            };

            xp.SetActive(false);
            xPieces.Add(xp);
        }

        var oPieces = new List<GameObject>();
        for(int i = 0; i < Mathf.FloorToInt(gameBoard.width * gameBoard.height / 2.0f); ++i) {
            var op = Instantiate(gameBoard.oPiece, gameBoard.transform);

            var opm = op.GetComponentInChildren<MeshRenderer>();
            opm.material.color = oColor;
            opm.transform.localScale = new Vector3 {
                x = gameBoard.squareSize.x * 0.8f,
                y = 0.2f,
                z = gameBoard.squareSize.y * 0.8f,
            };
            opm.transform.position = new Vector3 {
                x = gameBoard.squareSize.x / 2.0f,
                z = gameBoard.squareSize.y / 2.0f,
            };

            op.SetActive(false);
            oPieces.Add(op);
        }

        if(playerIsX) {
            turn = Turn.Player;
            _playerPieces.AddRange(xPieces);
            _aiPieces.AddRange(oPieces);

            currentTurnText.text = "Player";
            _playerColor = xColor;
            _aiColor = oColor;
        }
        else {
            turn = Turn.Ai;
            _aiPieces.AddRange(xPieces);
            _playerPieces.AddRange(oPieces);

            currentTurnText.text = "AI";
            _playerColor = oColor;
            _aiColor = xColor;
        }
    }

    void UpdateBoard() {
        if(!stateChanged) {
            return;
        }

        for(int i = 0; i < gameBoard.height; ++i) {
            for(int j = 0; j < gameBoard.width; ++j) {
                var idx = i * gameBoard.width + j;
                var s = gameBoard.state[idx];

                switch(s) {
                    case Board.State.NewPlayerPiece:
                        var pp = _playerPieces[_activePlayerPieces++];
                        pp.SetActive(true);
                        pp.transform.position = new Vector3 {
                            x = j * gameBoard.squareSize.x,
                            y = 0.2f,
                            z = i * gameBoard.squareSize.y,
                        };
                        gameBoard.state[idx] = Board.State.PlayerPiece;
                        break;
                    case Board.State.NewAiPiece:
                        var ap = _aiPieces[_activeAiPieces++];
                        ap.SetActive(true);
                        ap.transform.position = new Vector3 {
                            x = j * gameBoard.squareSize.x,
                            y = 0.2f,
                            z = i * gameBoard.squareSize.y,
                        };
                        gameBoard.state[idx] = Board.State.AiPiece;
                        break;

                    default: break;
                }
            }
        }
        stateChanged = false;
    }
}
