using UnityEngine;

namespace Assets.Source.UI.Model
{
    public class CharacterAppearanceModel
    {
        #region Attributes
        public string SkinID;
        public Color SkinColor;
        #endregion

        #region Properties
        #endregion

        #region Methods
        public CharacterAppearanceModel Clone()
        {
            return (CharacterAppearanceModel)MemberwiseClone();
        }
        #endregion
    }
}