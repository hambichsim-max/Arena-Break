using System.Collections;
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
        [SerializeField, Min(0f)] private float _chaseDistance = 60f;
        [SerializeField, Min(0f)] private float _attackDistance = 2f;
        [SerializeField, Min(0f)] private float _attackInterval = 2f;
        [SerializeField, Min(1)] private int _damage = 10;
        [SerializeField, Min(0f)] private float _deathDelay = 1f;

        private Health _health;
        private NavMeshAgent _agent;
        private Collider _body;
        private IDamageable _targetHealth;
        private State _state;
        private float _nextAttack;
        private System.Action<EnemyAI> _returnToPool;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _agent = GetComponent<NavMeshAgent>();
            _body = GetComponent<Collider>();
            SetTarget(_player);
        }

        public void SetTarget(Transform player)
        {
            _player = player;
            _targetHealth = player != null ? player.GetComponent<IDamageable>() : null;
        }

        internal void SetPoolReturn(System.Action<EnemyAI> returnToPool) => _returnToPool = returnToPool;

        private void OnEnable()
        {
            _state = State.Idle;
            _nextAttack = 0f;
            if (_body != null)
                _body.enabled = true;
            _health.Died += Die;
        }

        private void OnDisable()
        {
            _health.Died -= Die;
            StopAllCoroutines();
        }

        private void Update()
        {
            if (_state == State.Dead || !_agent.isOnNavMesh)
                return;

            float distance = _player != null
                ? Vector3.ProjectOnPlane(_player.position - transform.position, Vector3.up).magnitude
                : float.PositiveInfinity;
            _state = distance <= _attackDistance ? State.Attack
                : distance <= _chaseDistance ? State.Chase : State.Idle;

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
                    if (Time.time >= _nextAttack)
                    {
                        _nextAttack = Time.time + _attackInterval;
                        Debug.Log("Enemy attacked.", this);
                        _targetHealth?.TakeDamage(_damage);
                    }
                    break;
            }
        }

        private void Die()
        {
            _state = State.Dead;
            if (_body != null)
                _body.enabled = false;
            if (_agent.isOnNavMesh)
                _agent.isStopped = true;
            StartCoroutine(RemoveAfterDelay());
        }

        private IEnumerator RemoveAfterDelay()
        {
            yield return new WaitForSeconds(_deathDelay);
            if (_returnToPool != null)
                _returnToPool(this);
            else
                Destroy(gameObject);
        }
    }
}
