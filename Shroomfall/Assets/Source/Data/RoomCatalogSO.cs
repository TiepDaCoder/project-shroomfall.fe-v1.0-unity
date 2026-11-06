using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Source.Data
{
    [Serializable]
    public class RoomAsset
    {
        public string id;
        public Sprite icon;
        public GameObject roomPrefab;
    }

    [CreateAssetMenu(menuName = "World/Room Catalog")]
    public class RoomCatalogSO : ScriptableObject
    {
        #region Attributes
        [SerializeField]
        private List<RoomAsset> rooms;
        private Dictionary<string, RoomAsset> lookup;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void OnEnable()
        {
            BuildLookup();
        }

        public bool TryGet(
            string id,
            out RoomAsset asset)
        {
            return lookup.TryGetValue(id, out asset);
        }

        public IReadOnlyList<RoomAsset> GetAll()
        {
            return rooms;
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<string, RoomAsset>();

            foreach (var room in rooms)
            {
                if (room == null)
                    continue;

                if (string.IsNullOrEmpty(room.id))
                    continue;

                lookup[room.id] = room;
            }
        }
        #endregion
    }
}