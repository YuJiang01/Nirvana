namespace CacheUtils.DataDumperImport.DataStructures
{
    public sealed class Mapper
    {
        public PairGenomic PairGenomic = null; // null
        public string FromType;
        public string ToType;

        public override string ToString()
        {
            return $"Mapper: from: {FromType}, to: {ToType}";
        }
    }
}
