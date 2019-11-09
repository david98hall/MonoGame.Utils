using MonoGame.Extended.Shapes;
using System;

namespace MonoGame.Utils.Extensions
{
    public static class PolygonExtensions
    {

        /// <summary>
        /// Calculates the area of the polygon.
        /// </summary>
        /// <param name="polygon">The polygon in question.</param>
        /// <returns>The area of the polygon in question.</returns>
        public static float GetArea(this Polygon polygon)
        {
            float area = 0;

            // Shoelace formula
            var vertices = polygon.Vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                var j = (i + 1) % vertices.Length; // The index of the next vertex
                area += (vertices[i].X * vertices[j].Y) - (vertices[i].Y * vertices[j].X);
            }

            return Math.Abs(area) / 2;
        }

    }
}
