using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MouseButton {
    public const int Left   = 0;
    public const int Right  = 1;
    public const int Middle = 2;
}

public class GameManager : MonoBehaviour {
    public enum Turn {
        Player, AI
    }

    public Board gameBoard   = null;
    public Turn turn         = Turn.Player;
    public bool stateChanged = false;

    List<GameObject> _playerPieces = new List<GameObject>();
    List<GameObject> _aiPieces     = new List<GameObject>();
    List<GameObject> _triggers     = new List<GameObject>();

    int _activePlayerPieces = 0;
    int _activeAiPieces     = 0;
    int _activeTriggers     = 0;

    Coroutine _resetCoroutine = null;
    Coroutine _aiCoroutine    = null;

    private void Start() {
        Random.InitState(seed: (int)System.DateTime.Now.ToFileTimeUtc()); // @Temp
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

                _triggers.Add(t);
                _activeTriggers += 1;
                t.name = $"Trigger_{_activeTriggers}";
            }
        }
    }

    private void Update() {
        {
            var endGame = false;

            if(CheckWin(Turn.Player)) {
                Debug.Log($"Player wins!");
                endGame = true;
            }
            else if(CheckWin(Turn.AI)) {
                Debug.Log($"AI wins!");
                endGame = true;
            }
            else if(_activeTriggers <= 0) {
            // else if(_activePlayerPieces + _activeAiPieces == gameBoard.state.Length) {
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
                    // Debug.DrawRay(ray.origin, ray.direction * 20.0f, Color.red, duration: 5.0f);
                    return;
                }

                Debug.Log($"Hit: {hitInfo.collider.gameObject.name}");
                var t = hitInfo.collider.gameObject.GetComponent<Trigger>();
                if(gameBoard.state[t.index] != Board.State.Empty) {
                    // Debug.DrawRay(ray.origin, ray.direction * 20.0f, Color.red, duration: 5.0f);
                    return;
                }

                // Debug.DrawRay(ray.origin, ray.direction * 20.0f, Color.green, duration: 5.0f);
                gameBoard.state[t.index] = Board.State.NewPlayerPiece;
                t.gameObject.SetActive(false);
                _activeTriggers -= 1;

                stateChanged = true;
                turn = Turn.AI;
            }
        }
        else {
            if(_aiCoroutine == null) {
                _aiCoroutine = StartCoroutine(AiTurn());
            }
        }

        UpdateBoard();
    }

    IEnumerator AiTurn() {
        Debug.Log("AI Turn");
        yield return new WaitForSeconds(2.0f);

        do {
            var rand = Mathf.FloorToInt(Random.Range(0.0f, 1.0f) * 8.0f + 0.5f);
            if(!_triggers[rand].activeSelf) continue;

            Debug.Log($"AI chose: {rand + 1}");
            gameBoard.state[rand] = Board.State.NewAiPiece;
            _triggers[rand].SetActive(false);
            _activeTriggers -= 1;

            stateChanged = true;
        }
        while(!stateChanged);

        Debug.Log("Player Turn");
        turn = Turn.Player;
        _aiCoroutine = null;
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

    public bool CheckWin(Turn turnToCheck) {
        bool CheckRow(int row) {
            var state = gameBoard.state[row * Board.HEIGHT];
            var check = true;

            for(int j = 1; j < Board.WIDTH; ++j) {
                if(gameBoard.state[row * Board.HEIGHT + j] != state) {
                    check = false;
                    break;
                }
            }

            return check && state != Board.State.Empty;
            // return gameBoard.state[row] == gameBoard.state[row + 1] &&
            //     gameBoard.state[row + 1] == gameBoard.state[row + 2] &&
            //     gameBoard.state[row] != Board.State.Empty;
        }

        bool CheckColumn(int col) {
            var state = gameBoard.state[col];
            var check = true;

            for(int i = 1; i < Board.HEIGHT; ++i) {
                if(gameBoard.state[i * Board.HEIGHT + col] != state) {
                    check = false;
                    break;
                }
            }

            return check && state != Board.State.Empty;

            // return gameBoard.state[0 * Board.HEIGHT + col] == gameBoard.state[1 * Board.HEIGHT + col] &&
            //     gameBoard.state[1 * Board.HEIGHT + col] == gameBoard.state[2 * Board.HEIGHT + col] &&
            //     gameBoard.state[col] != Board.State.Empty;
        }

        bool CheckDiagonal(bool upwards) {
            var check = true;
            if(upwards) {
                var state = gameBoard.state[0];
                for(int i = 1; i < Board.HEIGHT; ++i) {
                    if(gameBoard.state[i * Board.HEIGHT + i] != state) {
                        check = false;
                        break;
                    }
                }
                return check && state != Board.State.Empty;
            }
            else {
                var state = gameBoard.state[Board.WIDTH - 1];
                for(
                    int i = 0, j = Board.WIDTH - 1;
                    i < Board.HEIGHT && j >= 0;
                    ++i, --j
                ) {
                    if(gameBoard.state[i * Board.HEIGHT + j] != state) {
                        check = false;
                        break;
                    }
                }
                return check && state != Board.State.Empty;
            }

        }

        // @Note: Horizontal check
        for(int i = 0; i < Board.HEIGHT; ++i) {
            var row = i * Board.HEIGHT;
            if(!CheckRow(i)) {
                continue;
            }

            if(gameBoard.state[row] == Board.State.PlayerPiece && turnToCheck == Turn.Player) {
                return true;
            }
            else if(gameBoard.state[row] == Board.State.AiPiece && turnToCheck == Turn.AI) {
                return true;
            }
        }

        // @Note: Vertical check
        for(int j = 0; j < Board.WIDTH; ++j) {
            if(!CheckColumn(j)) {
                continue;
            }

            if(gameBoard.state[j] == Board.State.PlayerPiece && turnToCheck == Turn.Player) {
                return true;
            }
            else if(gameBoard.state[j] == Board.State.AiPiece && turnToCheck == Turn.AI) {
                return true;
            }
        }

        // @Note: Diagonal checks
        if(CheckDiagonal(upwards: true)) {
            if(gameBoard.state[0] == Board.State.PlayerPiece && turnToCheck == Turn.Player) {
                return true;
            }
            else if(gameBoard.state[0] == Board.State.AiPiece && turnToCheck == Turn.AI) {
                return true;
            }
        }

        if(CheckDiagonal(upwards: false)) {
            if(gameBoard.state[Board.WIDTH - 1] == Board.State.PlayerPiece && turnToCheck == Turn.Player) {
                return true;
            }
            else if(gameBoard.state[Board.WIDTH - 1] == Board.State.AiPiece && turnToCheck == Turn.AI) {
                return true;
            }
        }

        return false;
    }


    IEnumerator ResetLevel() {
        yield return new WaitForSeconds(5.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
