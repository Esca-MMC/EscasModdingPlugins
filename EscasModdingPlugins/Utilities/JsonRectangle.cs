using Rectangle = System.Drawing.Rectangle;

namespace EscasModdingPlugins
{
    /// <summary>A basic <see cref="Rectangle"/> wrapper for convenient JSON serialization.</summary>
    public struct JsonRectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public JsonRectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>Creates an equivalent rectangle instance.</summary>
        /// <returns>A new rectangle matching this instance.</returns>
        public Rectangle AsRect() { return new Rectangle(X, Y, Width, Height); }
    }
}
