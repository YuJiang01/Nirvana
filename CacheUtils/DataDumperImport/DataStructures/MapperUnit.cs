using System;

namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class MapperUnit : SortableCoordinate, IEquatable<MapperUnit>
    {
        #region members

        public readonly MapperUnitType ID;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public MapperUnit(ushort referenceIndex, int start, int end, MapperUnitType id)
            : base(referenceIndex, start, end)
        {
            ID = id;
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return End.GetHashCode() ^ ID.GetHashCode() ^ ReferenceIndex.GetHashCode() ^ Start.GetHashCode();
        }

        public bool Equals(MapperUnit value)
        {
            if (this == null) throw new NullReferenceException();
            if (value == null) return false;
            if (this == value) return true;
            return ReferenceIndex == value.ReferenceIndex && End == value.End && Start == value.Start && ID == value.ID;
        }

        #endregion

        /// <summary>
        /// returns a string representation of our exon
        /// </summary>
        public override string ToString()
        {
            return $"mapper unit: {ReferenceIndex}: {Start} - {End}. ID: {ID}";
        }
    }

    public enum MapperUnitType : byte
    {
        CodingDna,
        Genomic,
        Unknown
    }
}
