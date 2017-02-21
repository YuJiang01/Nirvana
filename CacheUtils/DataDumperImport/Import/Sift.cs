using System.Collections.Generic;
using CacheUtils.DataDumperImport.Parser;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class Sift
    {
        #region members

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static Sift()
        {
            KnownKeys = new HashSet<string>
            {
                PolyPhen.AnalysisKey,
                PolyPhen.IsMatrixCompressedKey,
                PolyPhen.MatrixKey,
                PolyPhen.PeptideLengthKey,
                PolyPhen.SubAnalysisKey,
                PolyPhen.TranslationMD5Key
            };
        }

        /// <summary>
        /// parses the relevant data from each sift object
        /// </summary>
        public static DataStructures.Sift Parse(ObjectValue objectValue)
        {
            string matrix = null;

            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper sift object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case PolyPhen.AnalysisKey:
                    case PolyPhen.IsMatrixCompressedKey:
                    case PolyPhen.PeptideLengthKey:
                    case PolyPhen.SubAnalysisKey:
                    case PolyPhen.TranslationMD5Key:
                        break;
                    case PolyPhen.MatrixKey:
                        matrix = DumperUtilities.GetString(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return new DataStructures.Sift(matrix);
        }
    }
}
