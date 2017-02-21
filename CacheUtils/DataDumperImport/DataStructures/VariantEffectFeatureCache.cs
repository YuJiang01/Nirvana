namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class VariantEffectFeatureCache
    {
        public Intron[] Introns                                      = null;
        public ProteinFunctionPredictions ProteinFunctionPredictions = null;
        public TranscriptMapper Mapper                               = null;

        public string Peptide;        
        public string TranslateableSeq;
    }
}
