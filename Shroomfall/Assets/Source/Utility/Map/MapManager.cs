using Contract.Enum.WorldDomain;
using UnityEngine;

namespace Assets.Source.Utility.Map
{
    public class MapManager : MonoBehaviour
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private WorldBuilder worldBuilder;

        [Header("Save / Load Configuration")]
        [SerializeField] private string roomId = "village";
        [SerializeField] private string fileName = "village.json";

        [Header("Room Identity Data")]
        [SerializeField] private RoomType roomType = RoomType.PublicSpace;
        #endregion

        #region Properties
        #endregion

        #region Methods
        [ContextMenu("Export Layout to JSON")]
        public void ExportCurrentMap()
        {
            if (worldBuilder == null) return;
            worldBuilder.SaveWorldToJson(roomId, fileName, roomType);
        }

        [ContextMenu("Import Layout from JSON")]
        public void ImportMapFromFile()
        {
            if (worldBuilder == null) return;
            worldBuilder.LoadWorldFromJson(roomId, fileName);
        }
        #endregion
    }
}