using System.Collections.Generic;
using CacheUtils.DataDumperImport.Parser;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ProteinFunctionPredictions
    {
        #region members

        private const string PolyPhenKey       = "polyphen";
        private const string PolyPhenHumVarKey = "polyphen_humvar";
        private const string PolyPhenHumDivKey = "polyphen_humdiv";
        private const string SiftKey           = "sift";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static ProteinFunctionPredictions()
        {
            KnownKeys = new HashSet<string>
            {
                PolyPhenHumVarKey,
                PolyPhenHumDivKey,
                PolyPhenKey,
                SiftKey
            };
        }

        /// <summary>
        /// parses the relevant data from each protein function predictions object
        /// </summary>
        public static DataStructures.ProteinFunctionPredictions Parse(ObjectValue objectValue)
        {
            var predictions = new DataStructures.ProteinFunctionPredictions();

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
                    case PolyPhenHumDivKey:
                        // not used by default
                        break;
                    case PolyPhenKey:
                        if (DumperUtilities.IsUndefined(ad))
                        {
                            // do nothing
                        }
                        else
                        {
                            throw new GeneralException($"Could not handle the PolyPhen key: [{ad.GetType()}]");
                        }
                        break;
                    case PolyPhenHumVarKey:
                        // used by default
                        var polyPhenHumVarNode = ad as ObjectKeyValue;
                        if (polyPhenHumVarNode != null)
                        {
                            predictions.PolyPhen = PolyPhen.Parse(polyPhenHumVarNode.Value);
                        }
                        else if (DumperUtilities.IsUndefined(ad))
                        {
                            predictions.PolyPhen = null;
                        }
                        else
                        {
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case SiftKey:
                        var siftNode = ad as ObjectKeyValue;
                        if (siftNode != null)
                        {
                            predictions.Sift = Sift.Parse(siftNode.Value);
                        }
                        else if (DumperUtilities.IsUndefined(ad))
                        {
                            predictions.Sift = null;
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

            return predictions;
        }
    }
}
