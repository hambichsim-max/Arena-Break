using System;
using System.Collections;
using System.Collections.Generic;
using ArenaBreak.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace ArenaBreak.Core
{
    public sealed class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private WaveData[] _waves;
        [SerializeField] private Transform _spawnPoints;
        [SerializeField] private Transform _player;
        [SerializeField, Min(0f)] private float _initialDelay = 1f;
        [SerializeField, Min(0f)] private float _betweenWaves = 3f;
        [SerializeField, Min(0.1f)] private float _spawnSearchRadius = 3f;

        private readonly Dictionary<Health, Action> _alive = new();
        private readonly List<Transform> _points = new();
        private bool _started;

        public event Action<int> WaveStarted;
        public event Action<int> WaveCleared;
        public event Action AllWavesCleared;

        private void Awake()
        {
            if (_spawnPoints == null)
                return;
            foreach (Transform point in _spawnPoints)
                _points.Add(point);
        }

        private void Start() => StartWaves();

        public void StartWaves()
        {
            if (_started || !isActiveAndEnabled)
                return;
            if (_waves == null || _waves.Length == 0 || _points.Count == 0 || _player == null)
            {
                Debug.LogError("Assign waves, spawn points and player to WaveSpawner.", this);
                return;
            }
            _started = true;
            StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            yield return new WaitForSeconds(_initialDelay);
            for (int i = 0; i < _waves.Length; i++)
            {
                WaveData wave = _waves[i];
                if (wave == null || wave.EnemyPrefab == null)
                {
                    Debug.LogError($"Wave {i + 1} is missing its enemy prefab.", this);
                    yield break;
                }

                int number = i + 1;
                WaveStarted?.Invoke(number);
                for (int j = 0; j < wave.SpawnCount; j++)
                {
                    if (!TrySpawn(wave.EnemyPrefab))
                    {
                        Debug.LogError($"Wave {number} has no spawn point on the NavMesh.", this);
                        yield break;
                    }
                    if (j + 1 < wave.SpawnCount)
                        yield return new WaitForSeconds(wave.SpawnInterval);
                }

                yield return new WaitUntil(() => _alive.Count == 0);
                WaveCleared?.Invoke(number);
                if (number < _waves.Length)
                    yield return new WaitForSeconds(_betweenWaves);
            }

            Debug.Log("All waves cleared.", this);
            AllWavesCleared?.Invoke();
        }

        private bool TrySpawn(EnemyAI prefab)
        {
            int first = UnityEngine.Random.Range(0, _points.Count);
            NavMeshAgent agent = prefab.GetComponent<NavMeshAgent>();
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            for (int i = 0; i < _points.Count; i++)
            {
                Transform point = _points[(first + i) % _points.Count];
                if (!NavMesh.SamplePosition(point.position, out NavMeshHit hit, _spawnSearchRadius, filter))
                    continue;

                Vector3 position = hit.position + Vector3.up * agent.baseOffset;
                EnemyAI enemy = Instantiate(prefab, position, point.rotation);
                enemy.SetTarget(_player);
                Health health = enemy.GetComponent<Health>();
                Action onDied = () => Forget(health);
                _alive.Add(health, onDied);
                health.Died += onDied;
                return true;
            }
            return false;
        }

        private void Forget(Health health)
        {
            health.Died -= _alive[health];
            _alive.Remove(health);
        }

        private void OnDestroy()
        {
            foreach (var entry in _alive)
            {
                if (entry.Key != null)
                    entry.Key.Died -= entry.Value;
            }
            _alive.Clear();
        }
    }
}
