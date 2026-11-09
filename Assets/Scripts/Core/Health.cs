using System;
using UnityEngine;

namespace ArenaBreak.Core
{
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int _maxHealth = 100;
        [SerializeField, Min(0)] private int _currentHealth;

        public int MaxHealth => _maxHealth;
        public int CurrentHealth => _currentHealth;

        public event Action<int, int> HealthChanged;
        public event Action<Health> Died;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || _currentHealth <= 0)
                return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            bool died = _currentHealth == 0;
            HealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (died)
                Died?.Invoke(this);
        }
    }
}
