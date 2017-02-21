using System.Collections.Generic;
using CacheUtils.DataDumperImport.Parser;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class MapperPair
    {
        #region members

        private const string DataType = "Bio::EnsEMBL::Mapper::Pair";

        private const string FromKey = "from";
        private const string OriKey  = "ori";
        private const string ToKey   = "to";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static MapperPair()
        {
            KnownKeys = new HashSet<string>
            {
                FromKey,
                OriKey,
                ToKey
            };
        }

        /// <summary>
        /// parses the relevant data from each mapper pairs object
        /// </summary>
        private static DataStructures.MapperPair Parse(ObjectValue objectValue, ushort currentReferenceIndex)
        {
            DataStructures.MapperUnit from    = null;
            DataStructures.MapperUnit to      = null;

            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the mapper pair object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case FromKey:
                        var fromKeyNode = ad as ObjectKeyValue;
                        if (fromKeyNode != null)
                        {
                            from = MapperUnit.Parse(fromKeyNode.Value, currentReferenceIndex);
                        }
                        else
                        {
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case OriKey:
                        // skip
                        break;
                    case ToKey:
                        var toKeyNode = ad as ObjectKeyValue;
                        if (toKeyNode != null)
                        {
                            to = MapperUnit.Parse(toKeyNode.Value, currentReferenceIndex);
                        }
                        else
                        {
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return new DataStructures.MapperPair(from, to);
        }

        /// <summary>
        /// parses the relevant data from each mapper pairs object
        /// </summary>
        public static List<DataStructures.MapperPair> ParseList(List<AbstractData> abstractDataList, ImportDataStore dataStore)
        {
            var mapperPairs = DumperUtilities.GetPopulatedList<DataStructures.MapperPair>(abstractDataList.Count);

            for (int mapperPairIndex = 0; mapperPairIndex < abstractDataList.Count; mapperPairIndex++)
            {
                var ad = abstractDataList[mapperPairIndex];

                if (ad.DataType != DataType)
                {
                    throw new GeneralException(
                        $"Expected a mapper pair data type, but found the following data type: [{ad.DataType}]");
                }

                var mapperPairNode = ad as ObjectValue;
                if (mapperPairNode == null)
                {
                    throw new GeneralException(
                        $"Could not transform the AbstractData object into an ObjectValue: [{ad.GetType()}]");
                }

                var newMapperPair = Parse(mapperPairNode, dataStore.CurrentReferenceIndex);
                mapperPairs[mapperPairIndex] = newMapperPair;
            }

            return mapperPairs;
        }
    }
}
