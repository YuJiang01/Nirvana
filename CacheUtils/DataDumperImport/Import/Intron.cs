using System.Collections.Generic;
using CacheUtils.DataDumperImport.Parser;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class Intron
    {
        #region members

        private const string AdaptorKey  = "adaptor";
        private const string SeqNameKey  = "seqname";
        private const string NextKey     = "next";
        private const string DbIdKey     = "dbID";
        private const string PrevKey     = "prev";
        private const string AnalysisKey = "analysis";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static Intron()
        {
            KnownKeys = new HashSet<string>
            {
                Transcript.EndKey,
                Transcript.SliceKey,
                Transcript.StartKey,
                Transcript.StrandKey,
                AdaptorKey,
                SeqNameKey,
                NextKey,
                DbIdKey,
                PrevKey,
                AnalysisKey
            };
        }

        /// <summary>
        /// returns a new exon given an ObjectValue
        /// </summary>
        private static DataStructures.Intron Parse(ObjectValue objectValue)
        {
            var intron = new DataStructures.Intron();

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
                    case AdaptorKey:
                    case SeqNameKey:
                    case NextKey:
                    case DbIdKey:
                    case PrevKey:
                    case AnalysisKey:
                    case Transcript.SliceKey:
                        // not used
                        break;
                    case Transcript.EndKey:
                        intron.End = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.StartKey:
                        intron.Start = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.StrandKey:
                        TranscriptUtilities.GetStrand(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return intron;
        }

        /// <summary>
        /// parses the relevant data from each intron object
        /// </summary>
        public static DataStructures.Intron[] ParseList(List<AbstractData> abstractDataList)
        {
            var introns = new DataStructures.Intron[abstractDataList.Count];

            for (int intronIndex = 0; intronIndex < abstractDataList.Count; intronIndex++)
            {
                var objectValue = abstractDataList[intronIndex] as ObjectValue;
                if (objectValue == null)
                {
                    throw new GeneralException(
                        $"Could not transform the AbstractData object into an ObjectValue: [{abstractDataList[intronIndex].GetType()}]");
                }
                introns[intronIndex] = Parse(objectValue);
            }

            return introns;
        }
    }
}
