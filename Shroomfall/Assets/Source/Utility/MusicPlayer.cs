using Assets.Source.Data;
using Assets.Source.Service;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Source.Utility
{
    public class MusicPlayer : MonoBehaviour
    {
        #region Attributes
        [Header("Source")]
        [SerializeField] private AudioSource musicSource;

        [Header("Data")]
        [SerializeField] private MusicCatalogSO musicCatalog;

        [Header("Music Title")]
        [SerializeField] private GameObject textWaveGO;
        [SerializeField] private TMP_Text musicTitleText;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float fadeOutDuration = 0.8f;

        private Coroutine playlistCoroutine;
        private Coroutine titleFadeCoroutine;
        private int currentMusicIndex = -1;
        private string currentLocalizationKey;

        private LocalizeService localizeService;
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void Initialize(
            LocalizeService service)
        {
            localizeService = service;
        }

        public void PlayPlaylist()
        {
            StopPlaylist();
            playlistCoroutine = StartCoroutine(PlayPlaylistRoutine());
        }

        public void StopPlaylist()
        {
            if (playlistCoroutine != null)
            {
                StopCoroutine(playlistCoroutine);
                playlistCoroutine = null;
            }

            musicSource.Stop();
            musicSource.clip = null;
            currentMusicIndex = -1;
        }

        private IEnumerator PlayPlaylistRoutine()
        {
            while (true)
            {
                if (!PlayRandomTrack())
                {
                    playlistCoroutine = null;
                    yield break;
                }

                yield return new WaitWhile(() => musicSource != null && musicSource.isPlaying);
            }
        }

        private bool PlayRandomTrack()
        {
            if (musicSource == null || musicCatalog == null || musicCatalog.Entries == null)
                return false;

            var availableEntries = new List<MusicEntry>();
            foreach (MusicEntry entry in musicCatalog.Entries)
            {
                if (entry == null || entry.audio == null)
                    continue;

                if (entry.index != currentMusicIndex)
                    availableEntries.Add(entry);
            }

            // If only one valid song exists, allow it to repeat.
            if (availableEntries.Count == 0)
            {
                foreach (MusicEntry entry in musicCatalog.Entries)
                {
                    if (entry != null && entry.audio != null)
                        availableEntries.Add(entry);
                }
            }

            if (availableEntries.Count == 0)
                return false;

            MusicEntry selected = availableEntries[Random.Range(0, availableEntries.Count)];
            musicSource.Stop();
            musicSource.clip = selected.audio;
            musicSource.loop = false;
            musicSource.Play();
            currentMusicIndex = selected.index;
            currentLocalizationKey = selected.localizationKey;

            if (musicTitleText != null)
            {
                musicTitleText.text = $"{localizeService.Get("song.song-label")} {localizeService.Get(currentLocalizationKey)}";
                ShowMusicTitle();
            }

            return true;
        }

        private void ShowMusicTitle()
        {
            if (titleFadeCoroutine != null)
                StopCoroutine(titleFadeCoroutine);

            titleFadeCoroutine = StartCoroutine(MusicTitleFadeRoutine());
        }

        private IEnumerator MusicTitleFadeRoutine()
        {
            if (textWaveGO != null)
                textWaveGO.SetActive(true);

            yield return FadeTextAlpha(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(displayDuration);
            yield return FadeTextAlpha(1f, 0f, fadeOutDuration);

            if (textWaveGO != null)
                textWaveGO.SetActive(false);

            titleFadeCoroutine = null;
        }

        private IEnumerator FadeTextAlpha(
            float startAlpha,
            float targetAlpha,
            float duration)
        {
            Color color = musicTitleText.color;
            if (duration <= 0f)
            {
                color.a = targetAlpha;
                musicTitleText.color = color;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
                musicTitleText.color = color;
                yield return null;
            }

            color.a = targetAlpha;
            musicTitleText.color = color;
        }
        #endregion
    }
}