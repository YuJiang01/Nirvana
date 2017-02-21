using System;

namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class MapperPair : IEquatable<MapperPair>
    {
        #region members

        public readonly MapperUnit From; // null
        public readonly MapperUnit To;   // null

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public MapperPair(MapperUnit from, MapperUnit to)
        {
            From = from;
            To   = to;
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return From.GetHashCode() ^ To.GetHashCode();
        }

        public bool Equals(MapperPair value)
        {
            if (this == null) throw new NullReferenceException();
            if (value == null) return false;
            if (this == value) return true;
            return From.Equals(value.From) && To.Equals(value.To);
        }

        #endregion
    }
}

