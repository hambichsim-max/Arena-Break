using System;
using ArenaBreak.Controllers;
using ArenaBreak.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ArenaBreak.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public enum State { Ready, Playing, GameOver, Cleared }

        [SerializeField] private Health _playerHealth;
        [SerializeField] private WaveSpawner _waveSpawner;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private WeaponSystem _weaponSystem;
        [SerializeField, Min(0f)] private float _readyDuration = 2f;

        private InputAction _restart;
        private InputAction _quit;
        private float _playAt;
        private bool _ownsPause;

        public State CurrentState { get; private set; }
        public event Action<State> StateChanged;

        private void Awake()
        {
            _restart = new InputAction("Restart", InputActionType.Button, "<Keyboard>/r");
            _quit = new InputAction("Quit", InputActionType.Button, "<Keyboard>/escape");
            if (_playerHealth != null && _waveSpawner != null && _playerController != null && _weaponSystem != null)
                return;
            Debug.LogError("Assign player health, player controls, weapon and wave spawner.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            _playerHealth.Died += OnPlayerDied;
            _waveSpawner.AllWavesCleared += OnAllWavesCleared;
            _restart.Enable();
            _quit.Enable();
        }

        private void Start()
        {
            SetControls(false);
            _playAt = Time.unscaledTime + _readyDuration;
        }

        private void Update()
        {
            if (_quit.WasPressedThisFrame())
            {
                Application.Quit();
                return;
            }

            switch (CurrentState)
            {
                case State.Ready when Time.unscaledTime >= _playAt:
                    SetControls(true);
                    _waveSpawner.StartWaves();
                    ChangeState(State.Playing);
                    break;
                case State.GameOver:
                case State.Cleared:
                    if (_restart.WasPressedThisFrame())
                    {
                        ReleasePause();
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    }
                    break;
            }
        }

        private void OnPlayerDied() => Finish(State.GameOver);
        private void OnAllWavesCleared() => Finish(State.Cleared);

        private void Finish(State result)
        {
            if (CurrentState != State.Playing)
                return;
            SetControls(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _ownsPause = true;
            Time.timeScale = 0f;
            ChangeState(result);
        }

        private void ChangeState(State state)
        {
            CurrentState = state;
            StateChanged?.Invoke(state);
        }

        private void SetControls(bool active)
        {
            _playerController.enabled = active;
            _weaponSystem.enabled = active;
        }

        private void ReleasePause()
        {
            if (!_ownsPause)
                return;
            Time.timeScale = 1f;
            _ownsPause = false;
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
                _playerHealth.Died -= OnPlayerDied;
            if (_waveSpawner != null)
                _waveSpawner.AllWavesCleared -= OnAllWavesCleared;
            _restart?.Disable();
            _quit?.Disable();
            ReleasePause();
        }

        private void OnDestroy()
        {
            _restart?.Dispose();
            _quit?.Dispose();
        }
    }
}
