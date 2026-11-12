using Contract.Enum.MetaDomain.Effect;
using System;
using TMPro;

namespace Assets.Source.UI.Model
{
    [Serializable]
    public struct AttributeUIModel
    {
        public AttributeType Type;
        public TMP_Text ValueText;
    }
}