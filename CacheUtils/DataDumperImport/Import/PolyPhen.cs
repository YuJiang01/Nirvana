using System.Collections.Generic;
using CacheUtils.DataDumperImport.Parser;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class PolyPhen
    {
        #region members

        private static readonly HashSet<string> KnownKeys;

        internal const string AnalysisKey           = "analysis";
        internal const string IsMatrixCompressedKey = "matrix_compressed";
        internal const string MatrixKey             = "matrix";
        internal const string PeptideLengthKey      = "peptide_length";
        internal const string SubAnalysisKey        = "sub_analysis";
        internal const string TranslationMD5Key     = "translation_md5";

        #endregion

        // constructor
        static PolyPhen()
        {
            KnownKeys = new HashSet<string>
            {
                AnalysisKey,
                IsMatrixCompressedKey,
                MatrixKey,
                PeptideLengthKey,
                SubAnalysisKey,
                TranslationMD5Key
            };
        }

        /// <summary>
        /// parses the relevant data from each PolyPhen object
        /// </summary>
        public static DataStructures.PolyPhen Parse(ObjectValue objectValue)
        {
            string matrix = null;

            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper PolyPhen object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case AnalysisKey:
                    case IsMatrixCompressedKey:
                    case PeptideLengthKey:
                    case SubAnalysisKey:
                    case TranslationMD5Key:
                        break;
                    case MatrixKey:
                        matrix = DumperUtilities.GetString(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return new DataStructures.PolyPhen(matrix);
        }
    }
}
