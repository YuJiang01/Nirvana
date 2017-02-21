using System;

namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class Gene : SortableCoordinate, IEquatable<Gene>
    {
        #region members

        public readonly string StableId;        // set
        private readonly bool _onReverseStrand; // set

        #endregion

        public Gene(ushort referenceIndex, int start, int end, string stableId, bool onReverseStrand)
            : base(referenceIndex, start, end)
        {
            StableId        = stableId;
            _onReverseStrand = onReverseStrand;


        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return ReferenceIndex.GetHashCode() ^ Start.GetHashCode() ^ End.GetHashCode() ^
                   _onReverseStrand.GetHashCode() ^ StableId.GetHashCode();
        }

        public bool Equals(Gene value)
        {
            if (this == null) throw new NullReferenceException();
            if (value == null) return false;
            if (this == value) return true;
            return ReferenceIndex == value.ReferenceIndex && End == value.End && Start == value.Start &&
                   _onReverseStrand == value._onReverseStrand && StableId == value.StableId;
        }

        #endregion

        /// <summary>
        /// returns a string representation of our gene
        /// </summary>
        public override string ToString()
        {
            return $"gene: {ReferenceIndex}: {Start} - {End}. {StableId} ({(_onReverseStrand ? "R" : "F")})";
        }
    }
}
