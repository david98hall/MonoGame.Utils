using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using MonoGame.Utils.Geometry;

namespace MonoGame.Utils.Appearance
{
    public static class TextureUtils
    {

        public static Texture2D CreatePolygonTexture(
            Vector2[] vertices,
            float maxWidth,
            float maxHeight,
            Color color,
            GraphicsDevice graphicsDevice)
        {

            var texture = new Texture2D(
                graphicsDevice,
                (int)(maxWidth + 0.5f),
                (int)(maxHeight + 0.5f));

            var transparent = new Color(0, 0, 0, 0);

            var shapePosition = new Vector2(0, 0);

            // Color the rectangle
            Color[] data = new Color[texture.Width * texture.Height];
            int x;
            int y = 0;
            for (int i = 0; i < data.Length; i++)
            {
                // Calculate pixel position
                x = i % texture.Width;
                if (x == 0 && i != 0)
                {
                    y++;
                }
                var pixelPosition = new Vector2(x, y);

                var isWithinPolygon = GeometryUtils.IsWithinPolygon(
                    pixelPosition,
                    vertices,
                    shapePosition,
                    maxWidth,
                    maxHeight
                );

                // Only paint the pixels that are within the shape
                data[i] = isWithinPolygon
                    ? color
                    : transparent;
            }
            texture.SetData(data);

            return texture;
        }

        public static Texture2D CreatePolygonTexture(Polygon polygon, Color color, GraphicsDevice graphicsDevice)
        {
            var maxWidth = polygon.Right - polygon.Left;
            var maxHeight = polygon.Bottom - polygon.Top;
            return CreatePolygonTexture(polygon.Vertices, maxWidth, maxHeight, color, graphicsDevice);
        }

        public static Texture2D CreateRectangleTexture(float width, float height, Color color, GraphicsDevice graphicsDevice)
        {
            var rect = new RectangleF(0, 0, width, height);
            return CreatePolygonTexture(rect.GetCorners(), rect.Width, rect.Height, color, graphicsDevice);
        }

    }
}
