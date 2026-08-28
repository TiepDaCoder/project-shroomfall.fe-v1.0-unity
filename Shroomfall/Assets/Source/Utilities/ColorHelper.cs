using Contract.Common;
using UnityEngine;

/*
 
Color space conversion helper.*
Purpose:
Convert HSV values to Unity Color.
Extract HSV components from Unity Color.
*
Usage:
Used for appearance customization and network-safe color storage.
*
Notes:
HSV is preferred for user-facing color editing.
Unity Color remains the rendering format.
*/

namespace Assets.Source.Utilities
{
    public static class ColorHelper
    {
        #region Attributes
        #endregion

        #region Properties
        #endregion

        #region Methods
        public static HSV ToHSV(
            Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            return new HSV { H = h, S = s, V = v };
        }

        public static Color ToColor(
            HSV hsv)
        {
            return Color.HSVToRGB(hsv.H, hsv.S, hsv.V);
        }
        #endregion
    }
}