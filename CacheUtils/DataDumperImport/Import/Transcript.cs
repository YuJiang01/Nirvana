using System.Collections.Generic;
using CacheUtils.DataDumperImport.Parser;
using CacheUtils.DataDumperImport.Utilities;
using VariantAnnotation.DataStructures;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    public static class Transcript
    {
        #region members

        public const string DataType = "Bio::EnsEMBL::Transcript";

        private const string AttributesKey                  = "attributes";
        private const string BiotypeKey                     = "biotype";
        private const string CcdsKey                        = "_ccds";
        private const string CdnaCodingEndKey               = "cdna_coding_end";
        private const string CdnaCodingStartKey             = "cdna_coding_start";
        private const string CodingRegionEndKey             = "coding_region_end";
        private const string CodingRegionStartKey           = "coding_region_start";
        private const string CreatedDateKey                 = "created_date";
        internal const string DbIdKey                       = "dbID";
        private const string DescriptionKey                 = "description";
        private const string DisplayXrefKey                 = "display_xref";
        internal const string EndKey                        = "end";
        private const string ExternalDbKey                  = "external_db";
        private const string ExternalDisplayNameKey         = "external_display_name";
        private const string ExternalNameKey                = "external_name";
        private const string ExternalStatusKey              = "external_status";
        private const string GeneHgncKey                    = "_gene_hgnc";
        private const string GeneHgncIdKey                  = "_gene_hgnc_id";
        private const string GeneKey                        = "_gene";
        private const string GenePhenotypeKey               = "_gene_phenotype";
        private const string GeneStableIdKey                = "_gene_stable_id";
        private const string GeneSymbolKey                  = "_gene_symbol";
        private const string GeneSymbolSourceKey            = "_gene_symbol_source";
        private const string IsCanonicalKey                 = "is_canonical";
        private const string ModifiedDateKey                = "modified_date";
        private const string ProteinKey                     = "_protein";
        private const string RefseqKey                      = "_refseq";
        internal const string SliceKey                      = "slice";
        private const string SourceKey                      = "source";
        internal const string StableIdKey                   = "stable_id";
        internal const string StartKey                      = "start";
        internal const string StrandKey                     = "strand";
        private const string SwissProtKey                   = "_swissprot";
        private const string TransExonArrayKey              = "_trans_exon_array";
        private const string TranslationKey                 = "translation";
        private const string TremblKey                      = "_trembl";
        private const string UniParcKey                     = "_uniparc";
        private const string VariationEffectFeatureCacheKey = "_variation_effect_feature_cache";
        internal const string VersionKey                    = "version";
        private const string VepLazyLoadedKey               = "_vep_lazy_loaded";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static Transcript()
        {
            KnownKeys = new HashSet<string>
            {
                AttributesKey,
                BiotypeKey,
                CcdsKey,
                CdnaCodingEndKey,
                CdnaCodingStartKey,
                CodingRegionEndKey,
                CodingRegionStartKey,
                CreatedDateKey,
                DbIdKey,
                DescriptionKey,
                DisplayXrefKey,
                EndKey,
                ExternalDbKey,
                ExternalDisplayNameKey,
                ExternalNameKey,
                ExternalStatusKey,
                GeneHgncKey,
                GeneHgncIdKey,
                GeneKey,
                GenePhenotypeKey,
                GeneStableIdKey,
                GeneSymbolKey,
                GeneSymbolSourceKey,
                IsCanonicalKey,
                ModifiedDateKey,
                ProteinKey,
                RefseqKey,
                SliceKey,
                SourceKey,
                StableIdKey,
                StartKey,
                StrandKey,
                SwissProtKey,
                TransExonArrayKey,
                TranslationKey,
                TremblKey,
                UniParcKey,
                VariationEffectFeatureCacheKey,
                VersionKey,
                VepLazyLoadedKey
            };
        }

        /// <summary>
        /// parses the relevant data from each transcript
        /// </summary>
        public static void Parse(ObjectValue objectValue, int transcriptIndex, ImportDataStore dataStore)
        {
            var bioType = BioType.Unknown;

            SimpleInterval[] microRnas                                  = null;
            DataStructures.Gene gene                                    = null;
            DataStructures.Translation translation                      = null;
            DataStructures.VariantEffectFeatureCache variantEffectCache = null;

            bool onReverseStrand = false;
            bool isCanonical     = false;

            int compDnaCodingStart = -1;
            int compDnaCodingEnd   = -1;

            int start    = -1;
            int end      = -1;
            byte version = 1;

            string proteinId    = null;
            string geneStableId = null;
            string stableId     = null;

            string geneSymbol = null;
            int hgncId        = -1;

            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper transcript object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case CodingRegionStartKey:
                    case CodingRegionEndKey:
                    case CreatedDateKey:
                    case DescriptionKey:
                    case DisplayXrefKey:
                    case ExternalDbKey:
                    case ExternalDisplayNameKey:
                    case ExternalNameKey:
                    case ExternalStatusKey:
                    case GenePhenotypeKey:
                    case ModifiedDateKey:
                    case SourceKey:
                    case SwissProtKey:
                    case TremblKey:
                    case UniParcKey:
                    case VepLazyLoadedKey:
                    case SliceKey:
                    case TransExonArrayKey:
                    case CcdsKey:
                    case DbIdKey:
                    case GeneSymbolSourceKey:
                    case RefseqKey:
                        // not used
                        break;
                    case AttributesKey:
                        var attributesList = ad as ListObjectKeyValue;
                        if (attributesList != null) microRnas = Attribute.ParseList(attributesList.Values);
                        break;
                    case BiotypeKey:
                        bioType = TranscriptUtilities.GetBiotype(ad);
                        break;
                    case CdnaCodingEndKey:
                        compDnaCodingEnd = DumperUtilities.GetInt32(ad);
                        break;
                    case CdnaCodingStartKey:
                        compDnaCodingStart = DumperUtilities.GetInt32(ad);
                        break;
                    case EndKey:
                        end = DumperUtilities.GetInt32(ad);
                        break;
                    case GeneHgncIdKey:
                        var hgnc = DumperUtilities.GetString(ad);
                        if (hgnc != null && hgnc.StartsWith("HGNC:")) hgnc = hgnc.Substring(5);
                        if (hgnc == "-" || hgnc == "") hgnc = null;
                        
                        if (hgnc != null) hgncId = int.Parse(hgnc);
                        break;
                    case GeneSymbolKey:
                    case GeneHgncKey: // older key
                        geneSymbol = DumperUtilities.GetString(ad);
                        if (geneSymbol == "-" || geneSymbol == "") geneSymbol = null;
                        break;
                    case GeneKey:
                        var geneNode = ad as ObjectKeyValue;
                        if (geneNode != null)
                        {
                            gene = Gene.Parse(geneNode.Value, dataStore.CurrentReferenceIndex);
                        }
                        break;
                    case GeneStableIdKey:
                        geneStableId = DumperUtilities.GetString(ad);
                        if (geneStableId == "-" || geneStableId == "") geneStableId = null;
                        break;
                    case IsCanonicalKey:
                        isCanonical = DumperUtilities.GetBool(ad);
                        break;
                    case ProteinKey:
                        proteinId = DumperUtilities.GetString(ad);
                        if (proteinId == "-" || proteinId == "") proteinId = null;
                        break;
                    case StableIdKey:
                        stableId = DumperUtilities.GetString(ad);
                        if (stableId == "-" || stableId == "") stableId = null;
                        break;
                    case StartKey:
                        start = DumperUtilities.GetInt32(ad);
                        break;
                    case StrandKey:
                        onReverseStrand = TranscriptUtilities.GetStrand(ad);
                        break;
                    case TranslationKey:
                        var translationNode = ad as ObjectKeyValue;
                        if (translationNode != null)
                        {
                            translation = Translation.Parse(translationNode.Value, dataStore);
                        }
                        else if (DumperUtilities.IsUndefined(ad))
                        {
                            translation = null;
                        }
                        else
                        {
                            throw new GeneralException($"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case VariationEffectFeatureCacheKey:
                        var cacheNode = ad as ObjectKeyValue;
                        if (cacheNode == null)
                        {
                            throw new GeneralException($"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        variantEffectCache = VariantEffectFeatureCache.Parse(cacheNode.Value, dataStore);
                        break;
                    case VersionKey:
                        version = (byte)DumperUtilities.GetInt32(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            dataStore.Transcripts.Add(new DataStructures.Transcript(bioType, gene, translation, variantEffectCache,
                onReverseStrand, isCanonical, compDnaCodingStart, compDnaCodingEnd, dataStore.CurrentReferenceIndex,
                start, end, proteinId, geneStableId, stableId, geneSymbol, hgncId, version, microRnas));
        }
    }
}
