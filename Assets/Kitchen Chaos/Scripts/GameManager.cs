using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; } = null;

        public event EventHandler OnGameStateChanged;
        public event EventHandler OnGameTogglePaused;

        public enum State { WaitingToStart, ReadyToPlay, CountdownToStart, GamePlaying, GameOver }
        
        private State state;

        [SerializeField] private float waitingToStartTimerMax = 1f;
        [SerializeField] private float countdownToStartTimerMax = 3f;
        [SerializeField] private float gamePlayingTimerMax = 15f;
        private float waitingToStartTimer;
        private float countdownToStartTimer;
        private float gamePlayingTimer;
        private bool isGamePaused = false;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);

            state = State.WaitingToStart;
            waitingToStartTimer = waitingToStartTimerMax;
            countdownToStartTimer = countdownToStartTimerMax;
            gamePlayingTimer = gamePlayingTimerMax;
        }
        
        private void Start()
        {
            InputManager.Instance.OnPauseAction += (_, _) => TogglePauseGame();
        }

        private void Update()
        {
            switch (state)
            {
                case State.WaitingToStart:
                    waitingToStartTimer -= Time.deltaTime;
                    if (waitingToStartTimer < 0f)
                    {
                        ChangeGameState(State.ReadyToPlay);
                        InputManager.Instance.OnPrimaryInteractAction += StartGame;
                    }
                    break;

                case State.ReadyToPlay:
                    break;

                case State.CountdownToStart:
                    countdownToStartTimer -= Time.deltaTime;
                    if (countdownToStartTimer < 0f)
                        ChangeGameState(State.GamePlaying);
                    break; 

                case State.GamePlaying:
                    gamePlayingTimer -= Time.deltaTime;
                    if (gamePlayingTimer < 0f)
                        ChangeGameState(State.GameOver);
                    break;

                case State.GameOver: 
                    break;
            }
        }

        private void StartGame(object sender = null, EventArgs e = null)
        {
            if (state == State.WaitingToStart) return;

            if (state == State.ReadyToPlay)
            {
                ChangeGameState(State.CountdownToStart);
            }
            // unsubcribe
            InputManager.Instance.OnPrimaryInteractAction -= StartGame;
        }

        private void ChangeGameState(State gameState)
        {
            state = gameState;
            OnGameStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsGamePlaying => state == State.GamePlaying;
        public bool IsGameOver => state == State.GameOver;
        public bool IsCountdownActive => state == State.CountdownToStart;
        public bool IsReadyToPlay => state == State.ReadyToPlay;
        public float GetGamePlayingTimerNormalized() => gamePlayingTimer / gamePlayingTimerMax;
        public int GetCountdownTimerValue() => Mathf.CeilToInt  (countdownToStartTimer);
        public bool IsGamePaused => isGamePaused;

        public void TogglePauseGame()
        {
            isGamePaused = !isGamePaused;
            OnGameTogglePaused?.Invoke(this, EventArgs.Empty);

            // all the object calculation run based on Time, can easily controlled by timeScale
            // while acts a multiplier value and controls the overall game speed, by default it is 1
            if (isGamePaused)
            {
                Time.timeScale = 0; 
                // when set 0, all time step values becomes 0,
                // thus freezing all the game controlling
            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }
}