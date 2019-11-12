namespace Gzip
{
    /// <summary>
    /// DTO обрабатываемого блока данных
    /// </summary>
    /// <typeparam name="TChunk"></typeparam>
    internal class Chunk<TChunk>
    {
        public Chunk(long id, TChunk block)
        {
            Id = id;
            Block = block;
        }

        public long Id { get; }
        public TChunk Block { get; }
    }
}