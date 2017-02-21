using System.Collections.Generic;
using CacheUtils.DataDumperImport.Parser;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class Gene
    {
        #region members

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static Gene()
        {
            KnownKeys = new HashSet<string>
            {
                Transcript.EndKey,
                Transcript.StableIdKey,
                Transcript.StartKey,
                Transcript.StrandKey
            };
        }

        /// <summary>
        /// returns a new gene given an ObjectValue
        /// </summary>
        public static DataStructures.Gene Parse(ObjectValue objectValue, ushort currentReferenceIndex)
        {
            int start            = -1;
            int end              = -1;
            string stableId      = null;
            bool onReverseStrand = false;

            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper gene object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case Transcript.EndKey:
                        end = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.StableIdKey:
                        stableId = DumperUtilities.GetString(ad);
                        break;
                    case Transcript.StartKey:
                        start = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.StrandKey:
                        onReverseStrand = TranscriptUtilities.GetStrand(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return new DataStructures.Gene(currentReferenceIndex, start, end, stableId, onReverseStrand);
        }
    }
}
