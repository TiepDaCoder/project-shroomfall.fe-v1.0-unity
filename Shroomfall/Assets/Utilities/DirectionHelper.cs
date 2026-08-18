using Contract.Enum.EntityDomain;
using UnityEngine;

namespace Assets.Utilities
{
    public static class DirectionHelper
    {
        #region Attributes
        #endregion

        #region Properties
        #endregion

        #region Methods
        public static Vector2 DirectionToVector(
            EntityDirection direction)
        {
            return direction switch
            {
                EntityDirection.UP => Vector2.up,
                EntityDirection.DOWN => Vector2.down,
                EntityDirection.LEFT => Vector2.left,
                EntityDirection.RIGHT => Vector2.right,
                _ => Vector2.down
            };
        }
        #endregion
    }
}