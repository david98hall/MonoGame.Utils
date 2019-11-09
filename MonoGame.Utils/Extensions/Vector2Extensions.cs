using Microsoft.Xna.Framework;

namespace MonoGame.Utils.Extensions
{
    public static class MyVector2Extensions
    {
        public static Vector2 Copy(this Vector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
    }
}
