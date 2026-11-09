using ArenaBreak.Enemy;
using UnityEngine;

namespace ArenaBreak.Core
{
    [CreateAssetMenu(menuName = "Arena Break/Wave Data")]
    public sealed class WaveData : ScriptableObject
    {
        [SerializeField] private EnemyAI _enemyPrefab;
        [SerializeField, Min(1)] private int _spawnCount = 3;
        [SerializeField, Min(0f)] private float _spawnInterval = 1f;

        public EnemyAI EnemyPrefab => _enemyPrefab;
        public int SpawnCount => _spawnCount;
        public float SpawnInterval => _spawnInterval;
    }
}
