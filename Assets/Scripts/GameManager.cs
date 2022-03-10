using System.Collections;
using System.Collections.Generic;
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

    public Board gameBoard   = null;
    public bool stateChanged = false;
    public Turn startingTurn = Turn.Player;
    public Turn turn         = Turn.Player;

    List<GameObject> _playerPieces = new List<GameObject>();
    List<GameObject> _aiPieces     = new List<GameObject>();
    List<GameObject> _triggers     = new List<GameObject>();

    int _activePlayerPieces = 0;
    int _activeAiPieces     = 0;
    int _activeTriggers     = 0;

    Coroutine _resetCoroutine = null;
    float _aiWaitTime = 0.0f;

    private void Update() {
        {
            var endGame = false;

            if(gameBoard.CheckWin(Turn.Player)) {
                Debug.Log($"Player wins!");
                endGame = true;
            }
            else if(gameBoard.CheckWin(Turn.Ai)) {
                Debug.Log($"AI wins!");
                endGame = true;
            }
            else if(gameBoard.CheckTie()) {
                Debug.Log("Draw! Resetting...");
                endGame = true;
            }

            if(endGame) {
                if(_resetCoroutine == null) {
                    _resetCoroutine = StartCoroutine(ResetLevel());
                }
                return;
            }
        }

        if(turn == Turn.Player) {
            if(Input.GetMouseButtonDown(MouseButton.Left)) {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(!Physics.Raycast(ray, out var hitInfo, maxDistance: 20.0f)) {
                    return;
                }

                Debug.Log($"Hit: {hitInfo.collider.gameObject.name}");
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
            Debug.Log("AI turn");
        }
        else {
            if(_aiWaitTime >= 0.0f) {
                _aiWaitTime -= Time.deltaTime;
                return;
            }

            do {
                var bestMove = MinimaxAI.FindBestMove(gameBoard);
                if(!_triggers[bestMove].activeSelf) continue;

                Debug.Log($"AI chose: {bestMove + 1}");
                gameBoard.state[bestMove] = Board.State.NewAiPiece;
                _triggers[bestMove].SetActive(false);
                _activeTriggers -= 1;

                stateChanged = true;
            }
            while(!stateChanged);

            turn = Turn.Player;
            Debug.Log("Player turn");
        }

        UpdateBoard();
    }

    public void ResetGame(bool playerIsX = true) {
        Random.InitState(seed: (int)System.DateTime.Now.ToFileTimeUtc()); // @Temp
        _resetCoroutine = null;

        for(int i = 0; i < Board.HEIGHT; ++i) {
            for(int j = 0; j < Board.WIDTH; ++j) {
                var t = Instantiate(gameBoard.trigger, gameBoard.transform);
                t.transform.position = new Vector3 {
                    x = j * gameBoard.squareSize,
                    z = i * gameBoard.squareSize,
                };
                t.GetComponent<Trigger>().index = _activeTriggers;

                _triggers.Add(t);
                _activeTriggers += 1;
                t.name = $"Trigger_{_activeTriggers}";
            }
        }

        var xPieces = new List<GameObject>();
        for(int i = 0; i < Mathf.CeilToInt(Board.WIDTH * Board.HEIGHT / 2.0f); ++i) {
            var xp = Instantiate(gameBoard.xPiece, gameBoard.transform);
            xp.SetActive(false);
            xPieces.Add(xp);
        }

        var oPieces = new List<GameObject>();
        for(int i = 0; i < Mathf.FloorToInt(Board.WIDTH * Board.HEIGHT / 2.0f); ++i) {
            var op = Instantiate(gameBoard.oPiece, gameBoard.transform);
            op.SetActive(false);
            oPieces.Add(op);
        }

        if(playerIsX) {
            startingTurn = turn = Turn.Player;
            _playerPieces.AddRange(xPieces);
            _aiPieces.AddRange(oPieces);
        }
        else {
            startingTurn = turn = Turn.Ai;
            _aiPieces.AddRange(xPieces);
            _playerPieces.AddRange(oPieces);
        }
    }

    void UpdateBoard() {
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
        }
        stateChanged = false;
    }


    IEnumerator ResetLevel() {
        yield return new WaitForSeconds(5.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
