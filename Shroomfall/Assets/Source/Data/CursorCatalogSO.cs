using Assets.Source.Enum;
using UnityEngine;

namespace Assets.Source.Data
{
    [System.Serializable]
    public class CursorEntry
    {
        public CursorType type;
        public Texture2D texture;
    }

    [CreateAssetMenu(fileName = "CursorCatalog", menuName = "Data/Cursor Catalog")]
    public class CursorCatalogSO : ScriptableObject
    {
        #region Attributes
        [SerializeField] private CursorEntry[] entries;
        #endregion

        #region Properties
        #endregion

        public CursorCatalogSO() { }

        #region Methods
        public Texture2D Get(
            CursorType type)
        {
            foreach (CursorEntry entry in entries)
            {
                if (entry.type == type)
                    return entry.texture;
            }

            return null;
        }
        #endregion
    }
}