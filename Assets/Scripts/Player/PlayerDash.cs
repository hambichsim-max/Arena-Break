using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaBreak.Player
{
    public sealed class PlayerDash : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _distance = 5f;
        [SerializeField, Min(0.01f)] private float _duration = 0.2f;
        [SerializeField, Min(0f)] private float _cooldown = 1f;

        private InputAction _input;
        private Vector3 _direction;
        private float _remaining;
        private float _readyAt;

        private void Awake() => _input = new InputAction("Dash", binding: "<Keyboard>/leftShift");
        private void OnEnable() => _input.Enable();
        private void OnDisable()
        {
            _input.Disable();
            _remaining = 0f;
        }
        private void OnDestroy() => _input.Dispose();

        internal Vector3 GetDisplacement(Vector3 walking, float deltaTime)
        {
            if (_remaining <= 0f && Time.time >= _readyAt && _input.WasPressedThisFrame()
                && _duration > 0f && _distance > 0f)
            {
                _direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                _remaining = _duration;
                _readyAt = Time.time + _cooldown;
            }

            float step = Mathf.Min(_remaining, deltaTime);
            _remaining -= step;
            return _direction * (_distance / Mathf.Max(_duration, 0.01f) * step)
                + walking * (deltaTime - step);
        }
    }
}
