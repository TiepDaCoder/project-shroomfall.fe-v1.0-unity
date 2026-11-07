using UnityEngine;

namespace Assets.Source.Data
{
    [System.Serializable]
    public class MusicEntry
    {
        public AudioClip audio;
        public int index;
        public string localizationKey;
    }

    [CreateAssetMenu(fileName = "MusicCatalog", menuName = "Data/Music Catalog")]
    public class MusicCatalogSO : ScriptableObject
    {
        #region Attributes
        [SerializeField] private MusicEntry[] entries;
        #endregion

        #region Properties
        public MusicEntry[] Entries
        {
            get { return entries; }
        }
        #endregion

        public MusicCatalogSO() { }

        #region Methods
        public AudioClip Get(
            int index)
        {
            if (entries == null)
                return null;

            foreach (MusicEntry entry in entries)
            {
                if (entry != null && entry.index == index)
                    return entry.audio;
            }

            return null;
        }
        #endregion
    }
}