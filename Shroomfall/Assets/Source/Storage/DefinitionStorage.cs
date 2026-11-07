using Assets.Source.Utility;
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
            path = Path.Combine(Application.persistentDataPath, Configuration.DEFINITION_CACHE_FILE_NAME);
        }

        public static void Save(
            DefinitionSnapshotDTO snapshot)
        {
            var json = JsonConvert.SerializeObject(snapshot, Formatting.Indented, settings);
            File.WriteAllText(path, json);
        }

        public static DefinitionSnapshotDTO Load()
        {
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<DefinitionSnapshotDTO>(json, settings);
        }

        private static readonly JsonSerializerSettings settings = new()
        {
            Converters =
            {
                new ComponentInstanceConverter(),
                new ComponentDefinitionConverter()
            }
        };
        #endregion
    }
}