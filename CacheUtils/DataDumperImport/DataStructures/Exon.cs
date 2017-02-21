using System;

namespace CacheUtils.DataDumperImport.DataStructures
{
    public class Exon : SortableCoordinate, IEquatable<Exon>
    {
        public readonly bool OnReverseStrand;
        public readonly byte? Phase;    // 0, 1, 2 (null if not set)

        /// <summary>
        /// constructor
        /// </summary>
        public Exon(ushort referenceIndex, int start, int end, bool onReverseStrand, byte? phase)
            : base(referenceIndex, start, end)
        {
            OnReverseStrand = onReverseStrand;
            Phase           = phase;
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return ReferenceIndex.GetHashCode()  ^
                   Start.GetHashCode()           ^
                   End.GetHashCode()             ^
                   OnReverseStrand.GetHashCode() ^
                   Phase.GetHashCode();
        }

        public bool Equals(Exon value)
        {
            if (this == null) throw new NullReferenceException();
            if (value == null) return false;
            if (this == value) return true;
            return ReferenceIndex == value.ReferenceIndex && End == value.End && Start == value.Start &&
                   OnReverseStrand == value.OnReverseStrand && Phase == value.Phase;
        }

        #endregion

        /// <summary>
        /// returns a string representation of our exon
        /// </summary>
        public override string ToString()
        {
            return $"exon: {ReferenceIndex}: {Start} - {End}. ({(OnReverseStrand ? "R" : "F")}), phase: {Phase}";
        }
    }
}
