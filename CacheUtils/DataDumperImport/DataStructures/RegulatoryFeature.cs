using System;

namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class RegulatoryFeature : SortableCoordinate, IEquatable<RegulatoryFeature>
    {
        #region members

        public readonly string Id;
        public readonly string FeatureType;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public RegulatoryFeature(ushort referenceIndex, int start, int end, string id, string type)
            : base(referenceIndex, start, end)
        {
            Id          = id;
            FeatureType = type;
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return End.GetHashCode() ^ Id.GetHashCode() ^ Start.GetHashCode() ^ ReferenceIndex.GetHashCode();
        }

        public bool Equals(RegulatoryFeature value)
        {
            if (this == null) throw new NullReferenceException();
            if (value == null) return false;
            if (this == value) return true;
            return ReferenceIndex == value.ReferenceIndex && End == value.End && Start == value.Start && Id == value.Id;
        }

        #endregion

        /// <summary>
        /// returns a string representation of our regulatory element
        /// </summary>
        public override string ToString()
        {
            return $"{ReferenceIndex}\t{Start}\t{End}\t{Id}\t{FeatureType}";
        }
    }
}
