using Assets.Source.Data;
using Contract.DTO.Definition.LocalizationDomain;
using Contract.DTO.Definition.WorldDomain;
using Contract.Enum.WorldDomain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Source.Utility.Map
{
    [Serializable]
    public class RoomDataPackageDTO
    {
        public RoomDefinitionDTO room { get; set; } = new RoomDefinitionDTO();
        public List<CellDTO> cells { get; set; } = new List<CellDTO>();
        public List<EntitySpawnRuleDTO> entitySpawnRules { get; set; } = new List<EntitySpawnRuleDTO>();
    }

    public class WorldBuilder : MonoBehaviour
    {
        #region Attributes
        [Header("Server Metadata")]
        [SerializeField] private Tilemap collisionLayer;
        [SerializeField] private TileBase collisionMetaTile;
        [SerializeField] private Transform spawnContainer;

        [Header("Client Metadata")]
        [SerializeField]
        private RoomCatalogSO catalog;

        private GameObject currentRoom;
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void LoadRoom(
            string roomId)
        {
            if (!catalog.TryGet(roomId, out RoomAsset asset))
                return;

            if (asset.roomPrefab == null)
                return;

            // Unload old room
            if (currentRoom != null)
            {
                Destroy(currentRoom);
                currentRoom = null;
            }

            // Load new room
            currentRoom = Instantiate(asset.roomPrefab);
        }

        #region Save to Json
        public void SaveWorldToJson(
            string roomId,
            string fileName,
            RoomType roomType)
        {
            RoomDataPackageDTO package = new RoomDataPackageDTO
            {
                room = new RoomDefinitionDTO
                {
                    Id = roomId,
                    Type = roomType,
                    Presentation = new RoomPresentationDefinitionDTO
                    {
                        LocalizedText = new LocalizedTextDTO()
                    }
                }
            };

            package.cells = ExportWorldToDTO(roomId);
            ExportSpawnRulesToDTO(roomId, package.entitySpawnRules);
            if (package.cells.Count == 0 && package.entitySpawnRules.Count == 0)
                return;

            try
            {
                string fullPath = GetMapDataPath(fileName);
                string json = JsonConvert.SerializeObject(package, Formatting.Indented);
                File.WriteAllText(fullPath, json);
            }
            catch (Exception)
            {

            }
        }

        private List<CellDTO> ExportWorldToDTO(
            string roomId)
        {
            var exported = new List<CellDTO>();

            if (collisionLayer == null)
                return exported;

            collisionLayer.CompressBounds();
            BoundsInt bounds = collisionLayer.cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int pos = new(x, y, 0);

                    exported.Add(new CellDTO
                    {
                        RoomDefinitionID = roomId,
                        X = pos.x,
                        Y = pos.y,

                        // Gameplay layer
                        Z = 1,

                        Type = collisionLayer.HasTile(pos)
                            ? CellType.NonWalkable
                            : CellType.Walkable
                    });
                }
            }

            return exported;
        }

        private void ExportSpawnRulesToDTO(
            string roomId,
            List<EntitySpawnRuleDTO> ruleOutputList)
        {
            if (spawnContainer == null) return;

            SpawnAreaMarker[] markers = spawnContainer.GetComponentsInChildren<SpawnAreaMarker>();

            foreach (var marker in markers)
            {
                BoxCollider2D collider = marker.GetComponent<BoxCollider2D>();
                if (collider == null) continue;

                Vector3 localPos = marker.transform.localPosition;

                float leftEdge = localPos.x + collider.offset.x - (collider.size.x * 0.5f);
                float rightEdge = localPos.x + collider.offset.x + (collider.size.x * 0.5f);
                float bottomEdge = localPos.y + collider.offset.y - (collider.size.y * 0.5f);
                float topEdge = localPos.y + collider.offset.y + (collider.size.y * 0.5f);

                int minXGrid = Mathf.FloorToInt(leftEdge);
                int maxXGrid = Mathf.CeilToInt(rightEdge) - 1;
                int minYGrid = Mathf.FloorToInt(bottomEdge);
                int maxYGrid = Mathf.CeilToInt(topEdge) - 1;

                if (maxXGrid < minXGrid) maxXGrid = minXGrid;
                if (maxYGrid < minYGrid) maxYGrid = minYGrid;

                ruleOutputList.Add(new EntitySpawnRuleDTO
                {
                    ID = Guid.NewGuid(),
                    RoomDefinitionID = roomId,
                    EntityDefinitionID = marker.EntityID,
                    Type = marker.RuleType,
                    MinX = minXGrid,
                    MaxX = maxXGrid,
                    MinY = minYGrid,
                    MaxY = maxYGrid,
                    MinCount = marker.MinCount,
                    MaxCount = marker.MaxCount
                });
            }
        }
        #endregion

        #region Export to Json
        public void LoadWorldFromJson(
            string roomId,
            string fileName)
        {
            try
            {
                string fullPath = GetMapDataPath(fileName);
                if (!File.Exists(fullPath))
                    return;

                string json = File.ReadAllText(fullPath);
                RoomDataPackageDTO package = JsonConvert.DeserializeObject<RoomDataPackageDTO>(json);
                if (package == null)
                    return;

                ClearConfig();
                BuildWorld(package.cells);
                BuildSpawnAreas(package.entitySpawnRules);

            }
            catch (Exception)
            {

            }
        }

        private void BuildWorld(
            IEnumerable<CellDTO> cellData)
        {
            ClearConfig(clearSpawns: false);

            if (collisionLayer == null)
                return;

            foreach (var cell in cellData)
            {
                if (cell.Type != CellType.NonWalkable)
                    continue;

                collisionLayer.SetTile(
                    new Vector3Int(cell.X, cell.Y, 0),
                    collisionMetaTile);
            }
        }

        private void BuildSpawnAreas(
            List<EntitySpawnRuleDTO> rules)
        {
            if (spawnContainer != null)
            {
                for (int i = spawnContainer.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(spawnContainer.GetChild(i).gameObject);
                }
            }

            if (rules == null) return;

            foreach (var rule in rules)
            {
                GameObject areaObj = new GameObject($"SpawnArea_{rule.EntityDefinitionID}");
                if (spawnContainer != null) areaObj.transform.SetParent(spawnContainer);

                float sizeX = (rule.MaxX - rule.MinX) + 1f;
                float sizeY = (rule.MaxY - rule.MinY) + 1f;

                float localCenterX = rule.MinX + (sizeX * 0.5f);
                float localCenterY = rule.MinY + (sizeY * 0.5f);

                areaObj.transform.localPosition = new Vector3(localCenterX, localCenterY, 0f);

                SpawnAreaMarker marker = areaObj.AddComponent<SpawnAreaMarker>();
                marker.EntityID = rule.EntityDefinitionID;
                marker.RuleType = rule.Type;
                marker.MinCount = rule.MinCount;
                marker.MaxCount = rule.MaxCount;

                BoxCollider2D collider = areaObj.AddComponent<BoxCollider2D>();
                if (collider != null)
                {
                    collider.size = new Vector2(sizeX, sizeY);
                    collider.isTrigger = true;
                }
            }
        }
        #endregion

        #region Clear Current Config
        public void ClearConfig(
            bool clearSpawns = true)
        {
            collisionLayer?.ClearAllTiles();

            if (clearSpawns && spawnContainer != null)
            {
                for (int i = spawnContainer.childCount - 1; i >= 0; i--)
                    DestroyImmediate(spawnContainer.GetChild(i).gameObject);
            }
        }
        #endregion

        private static string GetMapDataPath(
            string fileName)
        {
            string directory = Path.Combine(
                Application.dataPath,
                "Resources",
                "meta",
                "room");

            Directory.CreateDirectory(directory);
            fileName = Path.GetFileNameWithoutExtension(fileName);
            return Path.Combine(directory, $"{fileName}.json");
        }
        #endregion
    }
}