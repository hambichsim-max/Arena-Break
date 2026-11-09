using ArenaBreak.Core;
using UnityEngine;
using UnityEngine.AI;

namespace ArenaBreak.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Health))]
    public sealed class EnemyAI : MonoBehaviour
    {
        private enum State { Idle, Chase, Attack, Dead }

        [SerializeField] private Transform _player;
        [SerializeField, Min(0f)] private float _chaseRange = 60f;
        [SerializeField, Min(0f)] private float _attackRange = 2f;
        [SerializeField, Min(0f)] private float _attackInterval = 2f;
        [SerializeField, Min(1)] private int _attackDamage = 10;
        [SerializeField, Min(0f)] private float _deathDelay = 1f;

        private NavMeshAgent _agent;
        private Health _health;
        private Collider _body;
        private IDamageable _playerHealth;
        private State _state;
        private float _nextAttackTime;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<Health>();
            _body = GetComponent<Collider>();
            SetPlayer(_player);
        }

        private void OnEnable() => _health.Died += OnDied;
        private void OnDisable() => _health.Died -= OnDied;

        public void SetPlayer(Transform player)
        {
            _player = player;
            _playerHealth = player != null ? player.GetComponentInParent<IDamageable>() : null;
        }

        private void Update()
        {
            if (_state == State.Dead || !_agent.isOnNavMesh)
                return;

            if (_player == null)
            {
                _state = State.Idle;
            }
            else
            {
                Vector3 offset = _player.position - transform.position;
                float distance = offset.magnitude;
                offset.y = 0f;
                if (offset.sqrMagnitude <= _attackRange * _attackRange &&
                    Mathf.Abs(_player.position.y - transform.position.y) <= _attackRange)
                    _state = State.Attack;
                else if (distance <= _chaseRange)
                    _state = State.Chase;
                else
                    _state = State.Idle;
            }

            switch (_state)
            {
                case State.Idle:
                    _agent.isStopped = true;
                    break;
                case State.Chase:
                    _agent.isStopped = false;
                    _agent.SetDestination(_player.position);
                    break;
                case State.Attack:
                    _agent.isStopped = true;
                    if (Time.time >= _nextAttackTime)
                    {
                        _nextAttackTime = Time.time + _attackInterval;
                        Debug.LogFormat(this, "Enemy attacks {0} for {1} damage.", _player.name, _attackDamage);
                        _playerHealth?.TakeDamage(_attackDamage);
                    }
                    break;
            }
        }

        private void OnDied(Health health)
        {
            _state = State.Dead;
            _agent.enabled = false;
            if (_body != null)
                _body.enabled = false;

            Destroy(gameObject, _deathDelay);
        }
    }
}
