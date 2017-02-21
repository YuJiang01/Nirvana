namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class Translation
    {
        public Exon EndExon          = null;
        public Exon StartExon        = null;

        public int End;
        public int Start;
        public byte Version;
    }
}
