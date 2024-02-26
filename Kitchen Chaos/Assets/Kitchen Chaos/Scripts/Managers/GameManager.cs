using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

namespace KC
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; } = null;
        public static ulong LocalClientID => Instance.NetworkManager.LocalClientId;

        public event EventHandler OnGameStateChanged;
        public event EventHandler OnGameTogglePaused;
        public event EventHandler<KeyValuePair<ulong, bool>> OnAnyPlayerToggleReady;
        public event EventHandler<ulong> OnAnyPlayerDisconnected;

        public enum State { WaitingToStart, ReadyToPlay, CountdownToStart, GamePlaying, GameOver }
        private NetworkVariable<State> state = new(State.WaitingToStart);

        [SerializeField] private float waitingToCountdownTimerMax = 1f;
        [SerializeField] private float countdownToStartTimerMax = 3f;
        [SerializeField] private float gamePlayingTimerMax = 150f;
        [SerializeField] private Transform rootSpawnPoint;
        [SerializeField] private NetworkObject playerPrefab;
        [field: SerializeField] public bool ResetPlayerStatesOnPaused { get; private set; } = false;

        [field: SerializeField, Obsolete] public bool Testing { get; private set; } = false;
        // using obsolete, so that finally it will be easier to remove all its instances
        public bool IsLocalPlayerReady { get; private set; } = false;

        // Network Sync vars
        private NetworkVariable<float> waitingToCountdownTimer = new(0);
        private NetworkVariable<float> countdownToStartTimer = new(0);
        private NetworkVariable<float> gamePlayingTimer = new(0);
        private NetworkVariable<bool> isGamePaused = new(false);

        // Server only vars - for network security check from server end
        // no need for network variable, as we want to check only from server side once
        private List<Transform> playerSpawnPoints;
        private Dictionary<ulong, bool> players = new(); // key:clientID, val:bool denotes if the resp player is ready or not
        private bool IsAllPlayersReady => players.Values.All(ready => ready); // check == true
        private bool IsAllPlayersNotReady => players.Values.All(ready => !ready); // check == false
        /*
         make sure check majority in float based comparison, so that 
         for even no. of players, half of votes is enuf to pause the game
         whereas for odd no.of players, more than half votes is required 
         */
        private bool IsMostPlayersReady => players.Values.Count(ready => ready) >= players.Count/2f; // check == true
        private bool IsMostPlayersNotReady => players.Values.Count(ready => !ready) >= players.Count/2f; // check == false

        public bool IsAllPlayersSameReady => IsAllPlayersReady || IsAllPlayersNotReady;

        private Transform GetSpawnPoint(int clientID)
        {
            if (playerSpawnPoints.Count == 0)
                return rootSpawnPoint;
            return playerSpawnPoints[clientID % playerSpawnPoints.Count];
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        private void Start()
        {
            // Client
            state.OnValueChanged += OnGameStateChangedNetworked;
            isGamePaused.OnValueChanged += OnPauseGameToggledNetworked;
            InputManager.Instance.OnPauseAction += (_, _) => ToggleReady();
            InputManager.Instance.OnPrimaryInteractAction += ToggleReady;
            
            NetworkManager.OnClientConnectedCallback += GameManager_OnClientConnectedCallback_ServerRpc;
            NetworkManager.OnClientDisconnectCallback += GameManager_OnClientDisconnectCallback;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return; // game manager should only be running from server end

            state.Value = State.WaitingToStart;
            waitingToCountdownTimer.Value = waitingToCountdownTimerMax;
            countdownToStartTimer.Value = countdownToStartTimerMax;
            gamePlayingTimer.Value = gamePlayingTimerMax;

            playerSpawnPoints = rootSpawnPoint.GetComponentsInChildren<Transform>().ToList();
            playerSpawnPoints.Remove(rootSpawnPoint);

            // Server Testing
            if (IsServer && Testing)
            {
                ChangeGameState(State.CountdownToStart);
                countdownToStartTimer.Value /= 3;
                gamePlayingTimer.Value = gamePlayingTimerMax *= 5;
            }
        }

        #region Player Spawner
        // there is no server callbacks when a client joins
        [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)] // need to be quickly implemented
        private void GameManager_OnClientConnectedCallback_ServerRpc(ulong clientID)
        {
            //this.Log("Client Connected");
            if (!IsServer) // adding check to be run by only server
                return; // for some reason this callback also keeps getting executed by client also

            if (players.ContainsKey(clientID))
                return; // already player initialized

            Transform sp = GetSpawnPoint((int)clientID);
            sp.GetPositionAndRotation(out Vector3 pos, out Quaternion rot);
            this.Log("Setting up Player spawning at: " + sp);

            if (NetworkManager.NetworkConfig.PlayerPrefab != null)
            {
                this.LogWarning("NetworkManager's PlayerPrefab config is not null, Changing Player Spawn Location after spawning!");
                NetworkObject playerNO = NetworkManager.SpawnManager.GetPlayerNetworkObject(clientID);
                playerNO.transform.SetPositionAndRotation(pos, rot);
            }
            else
            {
                NetworkObject playerNO = Instantiate(playerPrefab, pos, rot);
                // make sure to keep player-prefab set to null in NetworkObject
                playerNO.SpawnAsPlayerObject(clientID, true);
            }

            players.Add(clientID, false); // adding player to list after initialization
        }

        private void GameManager_OnClientDisconnectCallback(ulong clientID)
        {
            players.Remove(clientID);
            OnAnyPlayerDisconnected?.Invoke(this, clientID);

            if (IsServer) // incases if game is paused or unready state prev, it rechecks the votes again
                HandlePlayerVotes();
        }
        #endregion

        private void Update()
        {
            if (!IsServer) return;
            switch (state.Value)
            {
                case State.WaitingToStart:
                    break;

                case State.ReadyToPlay:
                    waitingToCountdownTimer.Value -= Time.deltaTime;
                    if (waitingToCountdownTimer.Value < 0f)
                        StartGame();
                    break;

                case State.CountdownToStart:
                    countdownToStartTimer.Value -= Time.deltaTime;
                    if (countdownToStartTimer.Value < 0f)
                        ChangeGameState(State.GamePlaying);
                    break; 

                case State.GamePlaying:
                    gamePlayingTimer.Value -= Time.deltaTime;
                    if (gamePlayingTimer.Value < 0f)
                    {
                        ChangeGameState(State.GameOver);
                    }
                    break;

                case State.GameOver: 
                    break;
            }
        }

        private void HandlePlayerVotes()
        {
            if (!IsServer) return;
            switch (state.Value)
            {
                case State.WaitingToStart:
                    if (IsAllPlayersReady)
                        ChangeGameState(State.ReadyToPlay);
                    break;

                case State.ReadyToPlay:
                    if (!IsAllPlayersReady) // in between phase to stop match countdown
                    {
                        waitingToCountdownTimer.Value = waitingToCountdownTimerMax;
                        // if not all players remains in ready state during this wait phase, it will reset count down
                        ChangeGameState(State.WaitingToStart);
                    }
                    break;

                case State.CountdownToStart:
                    break;

                case State.GamePlaying:
                    if (!IsGamePaused)
                    {
                        if (IsMostPlayersNotReady) // if most votes to pause the game
                            TogglePauseGame();
                    }
                    else
                    {
                        if (IsMostPlayersReady) // if most votes to unpause the game
                            TogglePauseGame();
                    }
                    break;

                case State.GameOver:
                    break;
                    // vote for play again synced later
            }
        }

        #region Player - Ready / Pause / Play Again (Sync)
        /* used for various purposes based on each state
            1. State.ReadyToPlay => ready up all players inititally to state the game
            2. State.GamePlaying => vote for pausing game in between playing state
            3. State.GameOver => play again as team option without going to menu screen
        */
        public void ToggleReady(object sender = null, EventArgs e = null)
        {
                
            IsLocalPlayerReady = !IsLocalPlayerReady;
            ToggleReadyServerRpc(IsLocalPlayerReady);
            //this.Log("Is Local Player Ready: " + IsLocalPlayerReady);
        }

        [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
        private void ToggleReadyServerRpc(bool isReady, ServerRpcParams rpcParams = default)
        {
            // thorough check from server end as well
            ulong clientID = rpcParams.Receive.SenderClientId;
            players[clientID] = isReady;
            ToggleReadyClientRpc(clientID, isReady);
            HandlePlayerVotes();
        }        

        [ClientRpc]
        private void ToggleReadyClientRpc(ulong clientID, bool isReady)
        {
            this.Log($"Player[{clientID}] Ready:{isReady}");
            
            // client side player-ready dict sync
            if (players.ContainsKey(clientID)) players[clientID] = isReady;
            else players.Add(clientID, isReady);

            OnAnyPlayerToggleReady?.Invoke(this, new KeyValuePair<ulong, bool>(clientID, isReady));
        }
        #endregion
        
        private void StartGame()
        {
            if (!IsServer) return;
            ChangeGameState(State.CountdownToStart);
            GameStartedClientRpc();
        }

        [ClientRpc]
        private void GameStartedClientRpc()
        {
            InputManager.Instance.OnPrimaryInteractAction -= ToggleReady; // unsubcribe
        }

        private void ChangeGameState(State gameState)
        {
            state.Value = gameState;
        }
        private void OnGameStateChangedNetworked(State oldState, State newState)
        {
            this.Log("state: " + state.Value);
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TogglePauseGame()
        {
            if (!IsServer) return; // server only callback

            isGamePaused.Value = !isGamePaused.Value;

            // all the object calculation run based on Time, can easily controlled by timeScale
            // while acts a multiplier value and controls the overall game speed, by default it is 1
            // when set 0, all time step values becomes 0, thus freezing all the game controlling
            if (IsGamePaused)
                Time.timeScale = 0;
            else
                Time.timeScale = 1;
        }
        private void OnPauseGameToggledNetworked(bool oldPauseVal, bool newPauseVal)
        {
            this.Log("Is Game Paused: " + isGamePaused.Value);
            OnGameTogglePaused?.Invoke(this, EventArgs.Empty);

            if (ResetPlayerStatesOnPaused) // reset player-ready dict
                foreach (var key in players.Keys.ToList())
                    players[key] = !IsGamePaused;
        }

        public bool IsGamePlaying => state.Value == State.GamePlaying;
        public bool IsGameOver => state.Value == State.GameOver;
        public bool IsCountdownActive => state.Value == State.CountdownToStart;
        public bool IsReadyToPlay => state.Value == State.ReadyToPlay;
        public bool IsWaitingToStart => state.Value == State.WaitingToStart;
        public float GetGamePlayingTimerNormalized() => gamePlayingTimer.Value / gamePlayingTimerMax;
        public float GetWaitingTimerNormalized() => waitingToCountdownTimer.Value / waitingToCountdownTimerMax;
        public int GetCountdownTimerValue() => Mathf.CeilToInt(countdownToStartTimer.Value);
        public bool IsGamePaused => isGamePaused.Value;

    }
}