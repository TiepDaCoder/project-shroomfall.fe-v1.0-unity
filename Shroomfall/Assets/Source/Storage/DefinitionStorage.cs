using Assets.Source.Utilities;
using Contract.DTO.Feature.Design.Response;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace Assets.Source.Storage
{
    public static class DefinitionStorage
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
                Configuration.DEFINITION_CACHE_FILE_NAME);
        }

        public static void Save(
            DefinitionSnapshotDTO snapshot)
        {
            var json = JsonConvert.SerializeObject(snapshot, Formatting.Indented);

            File.WriteAllText(path, json);
        }

        public static DefinitionSnapshotDTO Load()
        {
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);

            var data = JsonConvert.DeserializeObject<DefinitionSnapshotDTO>(json);

            return data;
        }

        public static void Clear()
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        #endregion
    }
}