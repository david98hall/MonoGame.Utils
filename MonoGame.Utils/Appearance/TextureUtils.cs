using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using MonoGame.Utils.Geometry;

namespace MonoGame.Utils.Appearance
{
    public static class TextureUtils
    {

        private delegate bool IsPointInShape(Vector2 point);

        #region Ellipses
        public static Texture2D CreateFilledEllipseTexture(
            float horizontalRadius,
            float verticalRadius,
            Color color,
            GraphicsDevice graphicsDevice)
        {
            return CreateShapeTexture(2 * horizontalRadius, 2 * verticalRadius, color, graphicsDevice, point =>
            {
                return GeometryUtils.IsWithinEllipse(
                    point,
                    horizontalRadius,
                    verticalRadius,
                    new Vector2(0, 0)
                );
            });
        }

        public static Texture2D CreateEllipseOutlineTexture(
            float horizontalRadius,
            float verticalRadius,
            float outlineThickness,
            Color color,
            GraphicsDevice graphicsDevice)
        {
            return CreateShapeTexture(2 * horizontalRadius, 2 * verticalRadius, color, graphicsDevice, point =>
            {
                return GeometryUtils.IsOnEllipseOutline(
                    point,
                    horizontalRadius,
                    verticalRadius,
                    outlineThickness,
                    new Vector2(0, 0)
                );
            });
        }

        #region Circles
        public static Texture2D CreateFilledCircleTexture(
            float radius,
            Color color,
            GraphicsDevice graphicsDevice)
        {
            return CreateFilledEllipseTexture(radius, radius, color, graphicsDevice);
        }

        public static Texture2D CreateCircleOutlineTexture(
            float radius,
            float outlineThickness,
            Color color,
            GraphicsDevice graphicsDevice)
        {
            return CreateEllipseOutlineTexture(radius, radius, outlineThickness, color, graphicsDevice);
        }
        #endregion

        #endregion

        #region Polygons
        public static Texture2D CreatePolygonTexture(Vector2[] vertices, Color color, GraphicsDevice graphicsDevice)
        {
            var polygon = new Polygon(vertices);
            var maxWidth = polygon.Right - polygon.Left;
            var maxHeight = polygon.Bottom - polygon.Top;
            return CreateShapeTexture(maxWidth, maxHeight, color, graphicsDevice,
                point => GeometryUtils.IsWithinPolygon(point, vertices, new Vector2(0, 0)));
        }

        public static Texture2D CreatePolygonTexture(Polygon polygon, Color color, GraphicsDevice graphicsDevice)
        {
            return CreatePolygonTexture(polygon.Vertices, color, graphicsDevice);
        }

        public static Texture2D CreateRectangleTexture(float width, float height, Color color, GraphicsDevice graphicsDevice)
        {
            var rect = new RectangleF(0, 0, width, height);
            return CreatePolygonTexture(rect.GetCorners(), color, graphicsDevice);
        }
        #endregion

        // Creates arbitrary shape textures depending on the passed IsPointInShape delegate
        private static Texture2D CreateShapeTexture(
            float maxWidth,
            float maxHeight,
            Color color,
            GraphicsDevice graphicsDevice,
            IsPointInShape isPointInShape)
        {
            var texture = new Texture2D(
                graphicsDevice,
                (int)(maxWidth + 0.5f),
                (int)(maxHeight + 0.5f)
            );

            var transparent = new Color(0, 0, 0, 0);

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

                // Only paint the pixels that are within the shape
                var isInShape = isPointInShape(pixelPosition);
                data[i] = isInShape
                    ? color
                    : transparent;
            }
            texture.SetData(data);

            return texture;
        }

    }
}
