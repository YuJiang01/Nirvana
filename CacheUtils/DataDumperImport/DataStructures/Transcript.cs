using System;
using System.Linq;
using System.Text;
using CacheUtils.DataDumperImport.Parser;
using VariantAnnotation.DataStructures;

namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class Transcript : SortableCoordinate, IEquatable<Transcript>
    {
        #region members

        public BioType BioType;
        public readonly SimpleInterval[] MicroRnas;
        public readonly Gene Gene;
        public Translation Translation;
        public VariantEffectFeatureCache VariantEffectCache;

        public readonly bool OnReverseStrand;
        public bool IsCanonical;

        public int CompDnaCodingStart;
        public int CompDnaCodingEnd;

        public readonly byte Version;

        public string ProteinId;
        public readonly string GeneStableId;
        public readonly string StableId;

        public string GeneSymbol;
        public readonly int HgncId;

        // ====================
        // conversion variables
        // ====================

        public readonly VariantAnnotation.DataStructures.Gene FinalGene;
        public int GeneIndex;

        public SimpleInterval[] FinalIntrons;
        public int[] IntronIndices;

        public SimpleInterval[] FinalMicroRnas;
        public int[] MicroRnaIndices;

        public CdnaCoordinateMap[] FinalCdnaMaps;

        public int SiftIndex       = -1;
        public int PolyPhenIndex   = -1;
        public int CdnaSeqIndex    = -1;
        public int PeptideSeqIndex = -1;

        #endregion

        public Transcript(BioType biotype, Gene gene, Translation translation, VariantEffectFeatureCache cache,
            bool onReverseStrand, bool isCanonical, int cdnaCodingStart, int cdnaCodingEnd, ushort referenceIndex,
            int start, int end, string proteinId, string geneStableId, string stableId, string geneSymbol, int hgncId,
            byte version, SimpleInterval[] microRnas)
            : base(referenceIndex, start, end)
        {
            BioType            = biotype;
            CompDnaCodingEnd   = cdnaCodingEnd;
            CompDnaCodingStart = cdnaCodingStart;
            Gene               = gene;
            GeneStableId       = geneStableId;
            GeneSymbol         = geneSymbol;
            HgncId             = hgncId;
            IsCanonical        = isCanonical;
            MicroRnas          = microRnas;
            OnReverseStrand    = onReverseStrand;
            ProteinId          = proteinId;
            StableId           = stableId;
            Translation        = translation;
            VariantEffectCache = cache;
            Version            = version;

            var entrezId = ImportDataStore.TranscriptSource == TranscriptDataSource.Ensembl
                ? CompactId.Empty
                : CompactId.Convert(geneStableId);

            var ensemblId = ImportDataStore.TranscriptSource == TranscriptDataSource.Ensembl
                ? CompactId.Convert(geneStableId)
                : CompactId.Empty;

            FinalGene = new VariantAnnotation.DataStructures.Gene(referenceIndex, start, end, onReverseStrand,
                geneSymbol, hgncId, entrezId, ensemblId, -1);
        }

        #region Equality Overrides

        // ReSharper disable once NonReadonlyFieldInGetHashCode
        public override int GetHashCode()
        {
            int hashCode = BioType.GetHashCode()         ^
                           End.GetHashCode()             ^
                           OnReverseStrand.GetHashCode() ^
                           ReferenceIndex.GetHashCode()  ^
                           Start.GetHashCode();

            if (GeneStableId != null) hashCode ^= GeneStableId.GetHashCode();
            if (StableId     != null) hashCode ^= StableId.GetHashCode();
            if (Gene         != null) hashCode ^= Gene.GetHashCode();

            return hashCode;
        }

        public bool Equals(Transcript value)
        {
            if (this == null) throw new NullReferenceException();
            if (value == null) return false;
            if (this == value) return true;
            return BioType            == value.BioType            &&
                   CompDnaCodingEnd   == value.CompDnaCodingEnd   &&
                   CompDnaCodingStart == value.CompDnaCodingStart &&
                   End                == value.End                &&
                   Gene               == value.Gene               &&
                   GeneStableId       == value.GeneStableId       &&
                   IsCanonical        == value.IsCanonical        &&
                   OnReverseStrand    == value.OnReverseStrand    &&
                   ProteinId          == value.ProteinId          &&
                   ReferenceIndex     == value.ReferenceIndex     &&
                   StableId           == value.StableId           &&
                   Start              == value.Start;
        }

        #endregion

        /// <summary>
        /// returns the start position of the coding region. Returns -1 if no translation was possible.
        /// </summary>
        private int GetCodingRegionStart()
        {
            // sanity check: make sure that translation is not null
            if (Translation == null) return -1;

            return Translation.StartExon.OnReverseStrand
                ? Translation.EndExon.End     - Translation.End   + 1
                : Translation.StartExon.Start + Translation.Start - 1;
        }

        /// <summary>
        /// returns the start position of the coding region. Returns -1 if no translation was possible.
        /// </summary>
        private int GetCodingRegionEnd()
        {
            // sanity check: make sure that translation is not null
            if (Translation == null) return -1;

            return Translation.StartExon.OnReverseStrand
                ? Translation.StartExon.End - Translation.Start + 1
                : Translation.EndExon.Start + Translation.End - 1;
        }

        /// <summary>
        /// returns the sum of the exon lengths
        /// </summary>
        private int GetTotalExonLength()
        {
            return FinalCdnaMaps.Sum(exon => exon.GenomicEnd - exon.GenomicStart + 1);
        }
        
        /// <summary>
        /// returns a string representation of our exon
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            byte proteinVersion = Translation?.Version ?? 1;

            // write the IDs
            sb.AppendLine($"Transcript\t{StableId}\t{Version}\t{ProteinId}\t{proteinVersion}\t{Gene.StableId}\t{(byte)BioType}");

            // write the transcript info
            var canonical = IsCanonical ? 'Y' : 'N';
            var startExonPhase = Translation?.StartExon.Phase.ToString();
            sb.AppendLine($"{ReferenceIndex}\t{Start}\t{End}\t{GetCodingRegionStart()}\t{GetCodingRegionEnd()}\t{CompDnaCodingStart}\t{CompDnaCodingEnd}\t{GetTotalExonLength()}\t{canonical}\t{startExonPhase}\t{GeneIndex}");

            // write the internal indices
            sb.AppendLine($"{CdnaSeqIndex}\t{PeptideSeqIndex}\t{SiftIndex}\t{PolyPhenIndex}");

            DumpIntrons(sb);
            DumpCdnaMaps(sb);
            DumpMicroRnas(sb);

            return sb.ToString();
        }

        private void DumpCdnaMaps(StringBuilder sb)
        {
            sb.AppendLine($"cDNA maps\t{FinalCdnaMaps.Length}");
            foreach (var cdnaMap in FinalCdnaMaps) sb.AppendLine(cdnaMap.ToString());
        }

        private void DumpIntrons(StringBuilder sb)
        {
            if (IntronIndices == null)
            {
                sb.AppendLine("Introns\t0");
                return;
            }

            sb.AppendLine($"Introns\t{IntronIndices.Length}");
            foreach (var index in IntronIndices) sb.AppendLine(index.ToString());
        }

        private void DumpMicroRnas(StringBuilder sb)
        {
            if (MicroRnaIndices == null)
            {
                sb.AppendLine("miRNAs\t0");
                return;
            }

            sb.AppendLine($"miRNAs\t{MicroRnaIndices.Length}");
            foreach (var index in MicroRnaIndices) sb.AppendLine(index.ToString());
        }
    }
}
