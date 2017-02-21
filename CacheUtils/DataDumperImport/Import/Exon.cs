using System.Collections.Generic;
using CacheUtils.DataDumperImport.Parser;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class Exon
    {
        #region members

        private const string EndPhaseKey = "end_phase";
        private const string PhaseKey    = "phase";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static Exon()
        {
            KnownKeys = new HashSet<string>
            {
                Transcript.EndKey,
                EndPhaseKey,
                PhaseKey,
                Transcript.StableIdKey,
                Transcript.StartKey,
                Transcript.StrandKey
            };
        }

        /// <summary>
        /// returns a new exon given an ObjectValue
        /// </summary>
        public static DataStructures.Exon Parse(ObjectValue objectValue, ushort currentReferenceIndex)
        {
            bool onReverseStrand = false;

            int end   = -1;
            byte? phase = null;
            int start = -1;

            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper mapper object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case Transcript.StableIdKey:
                    case EndPhaseKey:
                        break;
                    case Transcript.EndKey:
                        end = DumperUtilities.GetInt32(ad);
                        break;
                    case PhaseKey:
                        int phaseInt = DumperUtilities.GetInt32(ad);
                        if (phaseInt != -1) phase = (byte) phaseInt;
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

            return new DataStructures.Exon(currentReferenceIndex, start, end, onReverseStrand, phase);
        }
    }
}
