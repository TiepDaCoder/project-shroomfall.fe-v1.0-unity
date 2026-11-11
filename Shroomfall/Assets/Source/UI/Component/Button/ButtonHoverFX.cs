using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.Button
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class ButtonHoverFX : MonoBehaviour
    {
        [Header("Arrow")]
        [SerializeField] private Sprite arrowSprite;

        [Header("Layout")]
        [SerializeField] private float inset = 6f;
        [SerializeField] private float arrowSize = 16f;

        [Header("Animation")]
        [SerializeField] private float moveDistance = 5f;
        [SerializeField] private float duration = 0.08f;

        [SerializeField]
        private RectTransform[] arrows = new RectTransform[4];
        [SerializeField]
        private Image[] arrowImages = new Image[4];

        private Vector2[] restPos = new Vector2[4];
        private Vector2[] hoverPos = new Vector2[4];

        Coroutine routine;

        private void Awake()
        {
            InitializeArrow(0, 0);
            InitializeArrow(1, -90);
            InitializeArrow(2, 180);
            InitializeArrow(3, -270);

            ApplyLayout();
        }

        [ContextMenu("Apply Layout")]
        public void ApplyLayout()
        {
            LayoutArrow(0,
                new Vector2(0, 1),
                new Vector2(inset, -inset),
                new Vector2(+1, -1));

            LayoutArrow(1,
                new Vector2(1, 1),
                new Vector2(-inset, -inset),
                new Vector2(-1, -1));

            LayoutArrow(2,
                new Vector2(1, 0),
                new Vector2(-inset, inset),
                new Vector2(-1, +1));

            LayoutArrow(3,
                new Vector2(0, 0),
                new Vector2(inset, inset),
                new Vector2(+1, +1));
        }

        private void InitializeArrow(int index, float rotation)
        {
            if (arrows[index] == null || arrowImages[index] == null)
                return;

            arrows[index].localRotation = Quaternion.Euler(0, 0, rotation);

            arrowImages[index].sprite = arrowSprite;
            arrowImages[index].raycastTarget = false;
            arrowImages[index].enabled = false;

            if (arrowSprite != null)
                arrowSprite.texture.filterMode = FilterMode.Point;
        }

        private void LayoutArrow(
            int index,
            Vector2 anchor,
            Vector2 rest,
            Vector2 direction)
        {
            if (arrows[index] == null)
                return;

            RectTransform rt = arrows[index];

            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.one * arrowSize;

            restPos[index] = rest;
            hoverPos[index] = rest + direction.normalized * moveDistance;

            rt.anchoredPosition = rest;
        }

        public void Hover(bool state)
        {
            if (!Application.isPlaying)
                return;

            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            if (state)
            {
                for (int i = 0; i < 4; i++)
                {
                    arrowImages[i].enabled = true;
                    arrows[i].anchoredPosition = restPos[i];
                }

                routine = StartCoroutine(AnimateLoop());
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    arrowImages[i].enabled = false;
                    arrows[i].anchoredPosition = restPos[i];
                }
            }
        }

        IEnumerator AnimateLoop()
        {
            while (true)
            {
                // Snap inward
                for (int i = 0; i < 4; i++)
                {
                    if (arrows[i] != null)
                        arrows[i].anchoredPosition = hoverPos[i];
                }

                yield return new WaitForSeconds(duration);

                // Snap outward
                for (int i = 0; i < 4; i++)
                {
                    if (arrows[i] != null)
                        arrows[i].anchoredPosition = restPos[i];
                }

                yield return new WaitForSeconds(duration);
            }
        }
    }
}