using ArenaBreak.Core;
using UnityEngine;
using UnityEngine.AI;

namespace ArenaBreak.Enemy
{
    [RequireComponent(typeof(EnemyAI))]
    public sealed class ChargingEnemy : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _walkSpeed = 2f;
        [SerializeField, Min(0f)] private float _chargeSpeed = 8f;
        [SerializeField, Min(0f)] private float _chargeRange = 9f;
        [SerializeField, Min(0f)] private float _minimumChargeRange = 2f;
        [SerializeField, Min(0f)] private float _chargeDuration = 0.65f;
        [SerializeField, Min(0f)] private float _chargeCooldown = 4f;

        private Health _health;
        private NavMeshAgent _agent;
        private Transform _target;
        private float _chargeEnds;
        private float _readyAt;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _agent = GetComponent<NavMeshAgent>();
        }

        public void SetTarget(Transform target) => _target = target;

        private void OnEnable()
        {
            _agent.speed = _walkSpeed;
            _chargeEnds = 0f;
            _readyAt = 0f;
        }

        private void Update()
        {
            if (_target == null || _health.CurrentHealth <= 0 || !_agent.isOnNavMesh)
                return;

            float now = Time.time;
            if (now < _chargeEnds)
                return;

            _agent.speed = _walkSpeed;
            if (now < _readyAt)
                return;

            Vector3 offset = Vector3.ProjectOnPlane(_target.position - transform.position, Vector3.up);
            float distance = offset.magnitude;
            if (distance <= _minimumChargeRange || distance > _chargeRange)
                return;

            _chargeEnds = now + _chargeDuration;
            _readyAt = _chargeEnds + _chargeCooldown;
            _agent.speed = _chargeSpeed;
        }
    }
}
