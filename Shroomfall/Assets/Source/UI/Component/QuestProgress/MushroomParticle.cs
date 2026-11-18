using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.QuestProgress
{
    // Self-driven UI particle: bursts out in a random direction, slows, spins, fades, then destroys itself
    [RequireComponent(typeof(Graphic))]
    public class MushroomParticle : MonoBehaviour
    {
        #region Attributes
        [Header("Motion")]
        [SerializeField] private Vector2 speedRange = new(250f, 600f);
        [SerializeField] private float drag = 4f;
        [SerializeField] private float gravity = 300f;
        [SerializeField] private Vector2 spinRange = new(-360f, 360f);

        [Header("Look")]
        [SerializeField] private Vector2 lifetimeRange = new(0.6f, 1.1f);
        [SerializeField] private Vector2 startScaleRange = new(0.6f, 1.2f);
        [SerializeField] private float endScale = 0.3f;

        private RectTransform rectTransform;
        private Graphic graphic;
        private Color baseColor;

        private Vector2 velocity;
        private float spin;
        private float lifetime;
        private float startScale;
        private float elapsed;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            graphic = GetComponent<Graphic>();
            baseColor = graphic.color;

            // Particles are decoration, never block clicks
            graphic.raycastTarget = false;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(speedRange.x, speedRange.y);
            spin = Random.Range(spinRange.x, spinRange.y);
            lifetime = Random.Range(lifetimeRange.x, lifetimeRange.y);
            startScale = Random.Range(startScaleRange.x, startScaleRange.y);

            rectTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            rectTransform.localScale = Vector3.one * startScale;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;

            float t = Mathf.Clamp01(elapsed / lifetime);

            // Explode fast, then drag slows it while gravity pulls it down
            velocity *= Mathf.Exp(-drag * dt);
            velocity.y -= gravity * dt;

            rectTransform.anchoredPosition += velocity * dt;
            rectTransform.Rotate(0f, 0f, spin * dt);
            rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

            // Fade out over the second half of its life
            Color color = baseColor;
            color.a = baseColor.a * (1f - Mathf.InverseLerp(0.5f, 1f, t));
            graphic.color = color;

            if (t >= 1f)
                Destroy(gameObject);
        }
        #endregion
    }
}