using System;
using UnityEngine;

namespace ArenaBreak.Core
{
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public event Action<int, int> HealthChanged;
        public event Action Died;

        private void Awake() => _currentHealth = _maxHealth;

        internal void ResetHealth()
        {
            _currentHealth = _maxHealth;
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || _currentHealth <= 0)
                return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
            if (_currentHealth == 0)
                Died?.Invoke();
        }
    }
}
