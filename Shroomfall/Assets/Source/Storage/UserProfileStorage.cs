using Assets.Source.Utilities;
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
            path = Path.Combine(
                Application.persistentDataPath,
                Configuration.USER_PROFILE_FILE_NAME);
        }

        public static void Save(UserProfileDTO profile)
        {
            var json = JsonConvert.SerializeObject(profile, Formatting.Indented);

            File.WriteAllText(path, json);
        }

        public static UserProfileDTO Load()
        {
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);

            var data = JsonConvert.DeserializeObject<UserProfileDTO>(json);

            return data;
        }

        public static void Clear()
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        #endregion
    }

    [Serializable]
    public class UserProfileDTO
    {
        public string PreferredLocale;
        public string ScreenPresent;
    }
}