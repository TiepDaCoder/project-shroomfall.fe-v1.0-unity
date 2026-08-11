using Contract.DTO.Common;
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

namespace Assets.Utilities
{
    public static class ColorHelper
    {
        #region Attributes
        #endregion

        #region Properties
        #endregion

        #region Methods
        public static HSVDTO ToHSV(
            Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            return new HSVDTO { H = h, S = s, V = v };
        }

        public static Color ToColor(
            HSVDTO hsv)
        {
            return Color.HSVToRGB(hsv.H, hsv.S, hsv.V);
        }
        #endregion
    }
}