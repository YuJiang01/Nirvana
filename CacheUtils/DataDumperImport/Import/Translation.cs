using System.Collections.Generic;
using CacheUtils.DataDumperImport.Parser;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class Translation
    {
        #region members

        private const string AdaptorKey    = "adaptor";
        private const string EndExonKey    = "end_exon";
        private const string SequenceKey   = "seq";
        private const string StartExonKey  = "start_exon";
        private const string TranscriptKey = "transcript";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static Translation()
        {
            KnownKeys = new HashSet<string>
            {
                AdaptorKey,
                Transcript.DbIdKey,
                EndExonKey,
                Transcript.EndKey,
                SequenceKey,
                Transcript.StableIdKey,
                StartExonKey,
                Transcript.StartKey,
                TranscriptKey,
                Transcript.VersionKey
            };
        }

        /// <summary>
        /// parses the relevant data from each translation object
        /// </summary>
        public static DataStructures.Translation Parse(ObjectValue objectValue, ImportDataStore dataStore)
        {
            var translation = new DataStructures.Translation();

            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper mapper object: {ad.Key}");
                }

                // handle each key
                ObjectKeyValue exonNode;
                switch (ad.Key)
                {
                    case AdaptorKey:
                    case SequenceKey:
                    case Transcript.DbIdKey:
                    case Transcript.StableIdKey:
                    case TranscriptKey:
                        // skip this key
                        break;
                    case EndExonKey:
                        exonNode = ad as ObjectKeyValue;
                        if (exonNode != null)
                        {
                            var newExon = Exon.Parse(exonNode.Value, dataStore.CurrentReferenceIndex);
                            translation.EndExon = newExon;
                        }
                        break;
                    case StartExonKey:
                        exonNode = ad as ObjectKeyValue;
                        if (exonNode != null)
                        {
                            var newExon = Exon.Parse(exonNode.Value, dataStore.CurrentReferenceIndex);
                            translation.StartExon = newExon;
                        }
                        break;
                    case Transcript.EndKey:
                        translation.End = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.StartKey:
                        translation.Start = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.VersionKey:
                        translation.Version = (byte)DumperUtilities.GetInt32(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return translation;
        }
    }
}
