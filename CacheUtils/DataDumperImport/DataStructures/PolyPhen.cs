using System;

namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class PolyPhen : IEquatable<PolyPhen>
    {
        #region members

        public readonly string Matrix;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public PolyPhen(string matrix)
        {
            Matrix = matrix;
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return Matrix.GetHashCode();
        }

        public bool Equals(PolyPhen value)
        {
            if (this == null) throw new NullReferenceException();
            if (value == null) return false;
            if (this == value) return true;
            return Matrix == value.Matrix;
        }

        #endregion
    }
}
