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
        [SerializeField, Min(0f)] private float _intermission = 3f;
        [SerializeField, Min(0f)] private float _navMeshSampleDistance = 2f;

        private readonly Dictionary<Health, EnemyAI> _aliveEnemies = new();
        private Transform[] _points;

        public event Action<int> WaveStarted;
        public event Action<int> WaveCleared;
        public event Action AllWavesCleared;

        private void Awake()
        {
            if (_spawnPoints == null)
                return;

            _points = new Transform[_spawnPoints.childCount];
            for (int i = 0; i < _points.Length; i++)
                _points[i] = _spawnPoints.GetChild(i);
        }

        private void Start()
        {
            if (_player == null || _points == null || _points.Length == 0 || _waves == null || _waves.Length == 0)
            {
                Debug.LogError("WaveSpawner needs waves, a player and at least one spawn point.", this);
                return;
            }

            foreach (WaveData wave in _waves)
            {
                if (wave == null || wave.EnemyPrefab == null || wave.SpawnCount < 1)
                {
                    Debug.LogError("Every wave needs an enemy prefab and a positive spawn count.", this);
                    return;
                }
            }

            StartCoroutine(RunWaves());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            foreach (var entry in _aliveEnemies)
            {
                entry.Key.Died -= OnEnemyDied;
                if (entry.Value != null)
                    Destroy(entry.Value.gameObject);
            }

            _aliveEnemies.Clear();
        }

        private IEnumerator RunWaves()
        {
            for (int i = 0; i < _waves.Length; i++)
            {
                WaveData wave = _waves[i];
                int waveNumber = i + 1;
                Debug.LogFormat(this, "Wave {0} started.", waveNumber);
                WaveStarted?.Invoke(waveNumber);
                if (!isActiveAndEnabled)
                    yield break;

                var spawnDelay = new WaitForSeconds(wave.SpawnInterval);
                for (int spawned = 0; spawned < wave.SpawnCount; spawned++)
                {
                    if (!TrySpawn(wave.EnemyPrefab))
                    {
                        Debug.LogError("No spawn point could be placed on the NavMesh.", this);
                        yield break;
                    }

                    if (spawned + 1 < wave.SpawnCount)
                        yield return spawnDelay;
                }

                yield return new WaitUntil(() => _aliveEnemies.Count == 0);

                Debug.LogFormat(this, "Wave {0} cleared.", waveNumber);
                WaveCleared?.Invoke(waveNumber);
                if (!isActiveAndEnabled)
                    yield break;

                if (waveNumber < _waves.Length)
                    yield return new WaitForSeconds(_intermission);
            }

            Debug.Log("All waves cleared.", this);
            AllWavesCleared?.Invoke();
        }

        private bool TrySpawn(EnemyAI prefab)
        {
            int first = UnityEngine.Random.Range(0, _points.Length);
            for (int i = 0; i < _points.Length; i++)
            {
                Transform point = _points[(first + i) % _points.Length];
                if (!NavMesh.SamplePosition(point.position, out NavMeshHit hit, _navMeshSampleDistance, NavMesh.AllAreas))
                    continue;

                EnemyAI enemy = Instantiate(prefab, hit.position, point.rotation);
                enemy.SetPlayer(_player);
                Health health = enemy.GetComponent<Health>();
                _aliveEnemies.Add(health, enemy);
                health.Died += OnEnemyDied;
                return true;
            }

            return false;
        }

        private void OnEnemyDied(Health health)
        {
            health.Died -= OnEnemyDied;
            _aliveEnemies.Remove(health);
        }
    }
}
