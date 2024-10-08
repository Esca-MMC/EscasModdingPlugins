﻿using Microsoft.Xna.Framework;

namespace EscasModdingPlugins
{
    /// <summary>A basic integer-based <see cref="Vector2"/> wrapper for JSON serialization, allowing consistent formatting.</summary>
    public struct JsonVector2
    {
        public int X;
        public int Y;

        public JsonVector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>Creates an equivalent Vector2 instance.</summary>
        /// <returns>A new Vector2 matching this instance.</returns>
        public Vector2 AsVector2() { return new Vector2(X, Y); }
    }
}
