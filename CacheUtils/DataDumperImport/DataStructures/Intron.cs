using VariantAnnotation.DataStructures;

namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class Intron
    {
        public int Start;
        public int End;

        /// <summary>
        /// converts the current VEP exon into a Nirvana exon 
        /// </summary>
        public SimpleInterval Convert()
        {
            return new SimpleInterval(Start, End);
        }
    }
}
