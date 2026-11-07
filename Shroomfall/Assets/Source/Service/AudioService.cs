using Assets.Source.Storage;
using Assets.Source.Utility;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Service
{
    public class AudioService : IService
    {
        #region Attributes
        private readonly AudioSource musicSource;
        private readonly AudioSource ambientSource;
        private readonly AudioSource uiSource;
        private readonly MusicPlayer musicPlayer;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        public float MusicVolume { get; private set; } = 1f;
        public float AmbientVolume { get; private set; } = 1f;
        public float UIVolume { get; private set; } = 1f;
        public int CurrentMusicIndex { get; private set; } = -1;
        public bool IsMusicPlaying { get { return musicSource.isPlaying; } }
        #endregion

        public AudioService(
            AudioSource musicSource,
            AudioSource ambientSource,
            AudioSource uiSource,
            MusicPlayer musicPlayer)
        {
            this.musicSource = musicSource;
            this.ambientSource = ambientSource;
            this.uiSource = uiSource;
            this.musicPlayer = musicPlayer;
        }

        #region Methods
        public Task InitializeAsync()
        {
            // Reload profile
            var profile = UserProfileStorage.Load();
            if (profile?.PreferredAudio != null)
            {
                SetMusicVolume(profile.PreferredAudio.MusicVolume);
                SetAmbientVolume(profile.PreferredAudio.AmbientVolume);
                SetUIVolume(profile.PreferredAudio.UIVolume);
            }
            else
            {
                SetMusicVolume(1f);
                SetAmbientVolume(1f);
                SetUIVolume(1f);
            }

            musicSource.volume = MusicVolume;
            ambientSource.volume = AmbientVolume;
            uiSource.volume = UIVolume;

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void SetMusicVolume(
            float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            musicSource.volume = MusicVolume;

            // Update profile
            UserProfileStorage.UpdatePreferredAudio(MusicVolume, AmbientVolume, UIVolume);
        }

        public void SetAmbientVolume(
            float volume)
        {
            AmbientVolume = Mathf.Clamp01(volume);
            ambientSource.volume = AmbientVolume;

            // Update profile
            UserProfileStorage.UpdatePreferredAudio(MusicVolume, AmbientVolume, UIVolume);
        }

        public void SetUIVolume(
            float volume)
        {
            UIVolume = Mathf.Clamp01(volume);
            uiSource.volume = UIVolume;

            // Update profile
            UserProfileStorage.UpdatePreferredAudio(MusicVolume, AmbientVolume, UIVolume);
        }

        public void PlayMusic()
        {
            musicPlayer.PlayPlaylist();
        }

        public void StopMusic()
        {
            musicPlayer.StopPlaylist();
        }

        public void PlayAmbient(
            AudioClip clip)
        {
            if (clip == null)
                return;

            ambientSource.PlayOneShot(clip, AmbientVolume);
        }

        public void PlayUI(
            AudioClip clip)
        {
            if (clip == null)
                return;

            uiSource.PlayOneShot(clip, UIVolume);
        }

        #region Senders
        #endregion

        #region Receivers
        #endregion
        #endregion
    }
}