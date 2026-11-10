using ArenaBreak.Core;
using ArenaBreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ArenaBreak.UI
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private WeaponSystem _weapon;
        [SerializeField] private WaveSpawner _waves;
        [SerializeField] private GameManager _game;

        private RectTransform _root;
        private RectTransform _fill;
        private Text _ammo;
        private Text _wave;
        private Text _kills;
        private Text _message;
        private Font _font;

        private void Awake()
        {
            var overlay = new GameObject("HUD Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            _root = (RectTransform)overlay.transform;
            _root.SetParent(transform, false);
            overlay.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Image background = Add<Image>("Health", _root, Vector2.zero, new Vector2(150, 40), new Vector2(240, 24));
            background.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);
            Image fill = Add<Image>("Fill", background.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero);
            fill.color = new Color(0.85f, 0.18f, 0.2f);
            _fill = fill.rectTransform;
            _ammo = Label("Ammo", new Vector2(1, 0), new Vector2(-95, 40), 24);
            _wave = Label("Wave", new Vector2(0.5f, 1), new Vector2(0, -34), 24);
            _kills = Label("Kills", Vector2.one, new Vector2(-90, -34), 24);
            _message = Label("Result", new Vector2(0.5f, 0.5f), Vector2.zero, 32);
        }

        private void OnEnable()
        {
            _root.gameObject.SetActive(true);
            _health.HealthChanged += UpdateHealth;
            _weapon.AmmoChanged += UpdateAmmo;
            _waves.WaveStarted += UpdateWave;
            _waves.KillCountChanged += UpdateKills;
            _game.StateChanged += UpdateState;
            Refresh();
        }

        private void Start() => Refresh();

        private void OnDisable()
        {
            _health.HealthChanged -= UpdateHealth;
            _weapon.AmmoChanged -= UpdateAmmo;
            _waves.WaveStarted -= UpdateWave;
            _waves.KillCountChanged -= UpdateKills;
            _game.StateChanged -= UpdateState;
            _root.gameObject.SetActive(false);
        }

        private void Refresh()
        {
            UpdateHealth(_health.CurrentHealth, _health.MaxHealth);
            UpdateAmmo(_weapon.RemainingAmmo, _weapon.MagazineCapacity);
            UpdateWave(_waves.CurrentWave);
            UpdateKills(_waves.KillCount);
            UpdateState(_game.CurrentState);
        }

        private void UpdateHealth(int current, int max)
        {
            _fill.anchorMax = new Vector2(max > 0 ? Mathf.Clamp01((float)current / max) : 0f, 1);
            _fill.offsetMin = _fill.offsetMax = Vector2.zero;
        }

        private void UpdateAmmo(int current, int max) => _ammo.text = $"{current} / {max}";
        private void UpdateWave(int wave) => _wave.text = wave == 0 ? "Ready" : $"Wave {wave}";
        private void UpdateKills(int kills) => _kills.text = $"Kills: {kills}";

        private void UpdateState(GameManager.State state)
        {
            _message.gameObject.SetActive(state == GameManager.State.GameOver || state == GameManager.State.Cleared);
            _message.text = state == GameManager.State.Cleared ? "Cleared!\nPress R to restart" : "Game Over\nPress R to restart";
        }

        private Text Label(string name, Vector2 anchor, Vector2 position, int size)
        {
            Text label = Add<Text>(name, _root, anchor, position, new Vector2(340, 100));
            label.font = _font;
            label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            return label;
        }

        private static T Add<T>(string name, RectTransform parent, Vector2 anchor, Vector2 position, Vector2 size)
            where T : Graphic
        {
            var item = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)item.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            T graphic = item.AddComponent<T>();
            graphic.raycastTarget = false;
            return graphic;
        }
    }
}
