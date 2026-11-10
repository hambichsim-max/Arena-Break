using ArenaBreak.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaBreak.Player
{
    public sealed class WeaponSystem : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private Transform _cameraPivot;

        [Header("Weapon")]
        [SerializeField, Min(0f)] private float _fireInterval = 0.15f;
        [SerializeField, Min(0f)] private float _range = 50f;
        [SerializeField, Min(1)] private int _magazineCapacity = 12;
        [SerializeField, Min(0f)] private float _reloadDuration = 1.5f;

        [SerializeField, Min(1)] private int _damage = 10;

        [Header("Hit Marker")]
        [SerializeField] private Color _hitMarkerColor = Color.yellow;
        [SerializeField, Min(0.001f)] private float _hitMarkerSize = 0.08f;
        [SerializeField, Min(0f)] private float _hitMarkerDuration = 0.5f;

        private InputAction _attackAction;
        private InputAction _reloadAction;
        private Material _hitMarkerMaterial;
        private Coroutine _reloadCoroutine;
        private int _remainingAmmo;
        private float _nextFireTime;
        private bool _attackArmed;

        public int RemainingAmmo => _remainingAmmo;
        public int MagazineCapacity => _magazineCapacity;
        public event System.Action<int, int> AmmoChanged;

        private void Awake()
        {
            if (_inputActions == null)
            {
                Debug.LogError("WeaponSystem requires an InputActionAsset reference.", this);
                enabled = false;
                return;
            }

            if (_cameraPivot == null)
            {
                Debug.LogError("Assign the camera Transform to WeaponSystem.", this);
                enabled = false;
                return;
            }

            InputActionMap playerActionMap = _inputActions.FindActionMap("Player");
            if (playerActionMap == null)
            {
                Debug.LogError("InputActionAsset is missing the 'Player' action map.", this);
                enabled = false;
                return;
            }

            _attackAction = playerActionMap.FindAction("Attack");
            if (_attackAction == null)
            {
                Debug.LogError("The 'Player' action map must contain an 'Attack' action.", this);
                enabled = false;
                return;
            }

            _reloadAction = new InputAction("Reload", InputActionType.Button, "<Keyboard>/r");
            _remainingAmmo = _magazineCapacity;

            Shader markerShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (markerShader == null)
                markerShader = Shader.Find("Universal Render Pipeline/Lit");

            if (markerShader != null)
            {
                _hitMarkerMaterial = new Material(markerShader);
                SetMaterialColor(_hitMarkerMaterial, _hitMarkerColor);
            }
        }

        private void OnEnable()
        {
            if (_attackAction == null || _reloadAction == null)
                return;

            _attackAction.Enable();
            _reloadAction.Enable();
            _attackArmed = !_attackAction.IsPressed();
        }

        private void OnDisable()
        {
            _attackAction?.Disable();
            _reloadAction?.Disable();

            if (_reloadCoroutine != null)
            {
                StopCoroutine(_reloadCoroutine);
                _reloadCoroutine = null;
            }
        }

        private void OnDestroy()
        {
            _reloadAction?.Dispose();

            if (_hitMarkerMaterial != null)
                Destroy(_hitMarkerMaterial);
        }

        private void Update()
        {
            if (_reloadAction.WasPressedThisFrame())
                BeginReload();

            if (!_attackArmed)
            {
                if (!_attackAction.IsPressed())
                    _attackArmed = true;
            }

            if (_attackArmed && _reloadCoroutine == null && _attackAction.IsPressed() && Time.time >= _nextFireTime)
                Fire();
        }

        private void Fire()
        {
            if (_remainingAmmo <= 0)
            {
                BeginReload();
                return;
            }

            _nextFireTime = Time.time + _fireInterval;
            _remainingAmmo--;

            Ray ray = new Ray(_cameraPivot.position, _cameraPivot.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, _range))
            {
                hit.collider.GetComponentInParent<IDamageable>()?.TakeDamage(_damage);
                ShowHitMarker(hit.point);
            }

            AmmoChanged?.Invoke(_remainingAmmo, _magazineCapacity);
        }

        private void BeginReload()
        {
            if (_reloadCoroutine != null || _remainingAmmo >= _magazineCapacity)
                return;

            _reloadCoroutine = StartCoroutine(Reload());
        }

        private IEnumerator Reload()
        {
            yield return new WaitForSeconds(_reloadDuration);

            _remainingAmmo = _magazineCapacity;
            _reloadCoroutine = null;
            AmmoChanged?.Invoke(_remainingAmmo, _magazineCapacity);
        }

        private void ShowHitMarker(Vector3 position)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "HitMarker";
            marker.transform.position = position;
            marker.transform.localScale = Vector3.one * _hitMarkerSize;

            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                markerCollider.enabled = false;
                Destroy(markerCollider);
            }

            Renderer markerRenderer = marker.GetComponent<Renderer>();
            if (_hitMarkerMaterial != null)
                markerRenderer.sharedMaterial = _hitMarkerMaterial;
            else
                markerRenderer.material.color = _hitMarkerColor;

            Destroy(marker, _hitMarkerDuration);
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
        }
    }
}
