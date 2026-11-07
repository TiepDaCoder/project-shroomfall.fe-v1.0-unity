using Assets.Source.Enum;
using Assets.Source.Utility;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace Assets.Source.Storage
{
    public static class UserProfileStorage
    {
        #region Attributes
        private static string path;
        #endregion

        #region Properties
        #endregion

        #region Methods
        public static void Initialize()
        {
            path = Path.Combine(Application.persistentDataPath, Configuration.USER_PROFILE_FILE_NAME);
        }

        public static void UpdatePreferredScreenSize(
            ScreenPreset size)
        {
            var profile = Load();
            if (profile == null)
                profile = new UserProfileDTO();

            profile.PreferredScreenSize = size.ToString();
            Save(profile);
        }

        public static void UpdatePreferredScreenEffect(
            int index)
        {
            var profile = Load();
            if (profile == null)
                profile = new UserProfileDTO();

            profile.PreferredScreenEffect = index;
            Save(profile);
        }

        public static void UpdatePreferredLocale(
            string locale)
        {
            var profile = Load();
            if (profile == null)
                profile = new UserProfileDTO();

            profile.PreferredLocale = locale;
            Save(profile);
        }

        public static void UpdatePreferredAudio(
            float musicVolume,
            float ambientVolume,
            float uiVolume)
        {
            var profile = Load();
            if (profile == null)
                profile = new UserProfileDTO();

            profile.PreferredAudio = new PreferredAudio()
            {
                MusicVolume = musicVolume,
                AmbientVolume = ambientVolume,
                UIVolume = uiVolume
            };
            Save(profile);
        }

        public static UserProfileDTO Load()
        {
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<UserProfileDTO>(json);
            return data;
        }

        private static void Save(
            UserProfileDTO profile)
        {
            var json = JsonConvert.SerializeObject(profile, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        #endregion
    }

    [Serializable]
    public class UserProfileDTO
    {
        public string PreferredLocale;
        public string PreferredScreenSize;
        public int PreferredScreenEffect;
        public PreferredAudio PreferredAudio;
    }

    [Serializable]
    public class PreferredAudio
    {
        public float MusicVolume;
        public float AmbientVolume;
        public float UIVolume;
    }
}