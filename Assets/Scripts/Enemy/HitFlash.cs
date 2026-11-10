using ArenaBreak.Core;
using UnityEngine;

namespace ArenaBreak.Enemy
{
    [RequireComponent(typeof(Health), typeof(MeshRenderer))]
    public sealed class HitFlash : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private Health _health;
        private MeshRenderer _mesh;
        private MaterialPropertyBlock _previous;
        private MaterialPropertyBlock _flash;
        private int _lastHealth;
        private float _until;
        private bool _flashing;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _mesh = GetComponent<MeshRenderer>();
            _previous = new MaterialPropertyBlock();
            _flash = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            _lastHealth = _health.CurrentHealth;
            _health.HealthChanged += OnHealthChanged;
        }

        private void Start() => _lastHealth = _health.CurrentHealth;

        private void OnHealthChanged(int current, int max)
        {
            bool damaged = current < _lastHealth;
            _lastHealth = current;
            if (!damaged)
                return;
            if (!_flashing)
            {
                _mesh.GetPropertyBlock(_previous);
                _mesh.GetPropertyBlock(_flash);
                _flash.SetColor(BaseColor, Color.white);
                _mesh.SetPropertyBlock(_flash);
                _flashing = true;
            }
            _until = Time.unscaledTime + 0.05f;
        }

        private void Update()
        {
            if (_flashing && Time.unscaledTime >= _until)
                RestoreColor();
        }

        private void OnDisable()
        {
            _health.HealthChanged -= OnHealthChanged;
            RestoreColor();
        }

        private void RestoreColor()
        {
            if (!_flashing)
                return;
            _mesh.SetPropertyBlock(_previous);
            _flashing = false;
        }
    }
}
