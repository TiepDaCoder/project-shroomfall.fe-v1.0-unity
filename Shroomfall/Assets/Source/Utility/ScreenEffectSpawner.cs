using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.Utility
{
    public class ScreenEffectSpawner : MonoBehaviour
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject leafPrefab;
        [SerializeField] private GameObject flowerPrefab;
        [SerializeField] private GameObject mushroomPrefab;

        [Header("Spawning")]
        [SerializeField, Min(0.01f)] private float spawnInterval = 0.4f;
        [SerializeField, Min(1)] private int maxParticles = 25;

        [Header("Movement")]
        [SerializeField] private Vector2 movementDirection = new(-1f, -1f);
        [SerializeField, Min(0f)] private float minSpeed = 60f;
        [SerializeField, Min(0f)] private float maxSpeed = 120f;

        [Header("Appearance")]
        [SerializeField] private Vector2 particleSize = new(24f, 24f);
        [SerializeField] private float minRotationSpeed = -60f;
        [SerializeField] private float maxRotationSpeed = 60f;

        private readonly List<ParticleData> activeParticles = new();
        private RectTransform spawnArea;
        private GameObject currentPrefab;
        private float spawnTimer;
        private bool isPlaying;

        private class ParticleData
        {
            public RectTransform Rect;
            public Vector2 Velocity;
            public float RotationSpeed;
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            spawnArea = canvas != null ? canvas.transform as RectTransform : null;
        }

        private void OnEnable()
        {
            Play();
        }

        private void OnDisable()
        {
            Stop();
        }

        private void Update()
        {
            if (!isPlaying || spawnArea == null || currentPrefab == null)
                return;

            spawnTimer += Time.deltaTime;
            while (spawnTimer >= spawnInterval)
            {
                spawnTimer -= spawnInterval;
                if (activeParticles.Count < maxParticles)
                    SpawnParticle();
            }

            UpdateParticles();
        }

        public void SetEffect(
            int index)
        {
            switch (index)
            {
                case 0:
                    currentPrefab = leafPrefab;
                    break;

                case 1:
                    currentPrefab = flowerPrefab;
                    break;

                case 2:
                    currentPrefab = mushroomPrefab;
                    break;

                default:
                    currentPrefab = null;
                    Stop();
                    return;
            }

            spawnTimer = 0f;
            Play();
        }

        public void Play()
        {
            isPlaying = true;
            spawnTimer = 0f;
        }

        public void Stop()
        {
            isPlaying = false;

            foreach (ParticleData particle in activeParticles)
            {
                if (particle.Rect != null)
                    Destroy(particle.Rect.gameObject);
            }

            activeParticles.Clear();
        }

        private void SpawnParticle()
        {
            Rect bounds = spawnArea.rect;
            bool spawnFromTop = Random.value < 0.5f;
            Vector2 spawnPosition = spawnFromTop
                ? new Vector2(Random.Range(bounds.xMin, bounds.xMax), bounds.yMax)
                : new Vector2(bounds.xMax, Random.Range(bounds.yMin, bounds.yMax));

            GameObject instance = Instantiate(currentPrefab, spawnArea);
            RectTransform rect = instance.GetComponent<RectTransform>();

            if (rect == null)
            {
                Destroy(instance);
                return;
            }

            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = spawnPosition;
            rect.sizeDelta = particleSize;
            rect.localScale = Vector3.one;

            Image image = instance.GetComponent<Image>();
            if (image != null)
                image.raycastTarget = false;

            activeParticles.Add(new ParticleData
            {
                Rect = rect,
                Velocity = movementDirection.normalized * Random.Range(minSpeed, maxSpeed),
                RotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed)
            });
        }

        private void UpdateParticles()
        {
            Rect bounds = spawnArea.rect;
            float margin = Mathf.Max(particleSize.x, particleSize.y);

            for (int i = activeParticles.Count - 1; i >= 0; i--)
            {
                ParticleData particle = activeParticles[i];
                if (particle.Rect == null)
                {
                    activeParticles.RemoveAt(i);
                    continue;
                }

                particle.Rect.anchoredPosition += particle.Velocity * Time.deltaTime;
                particle.Rect.Rotate(0f, 0f, particle.RotationSpeed * Time.deltaTime);
                Vector2 position = particle.Rect.anchoredPosition;

                if (position.x < bounds.xMin - margin || position.y < bounds.yMin - margin)
                {
                    Destroy(particle.Rect.gameObject);
                    activeParticles.RemoveAt(i);
                }
            }
        }
        #endregion
    }
}