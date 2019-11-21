namespace Gzip
{
    /// <summary>
    /// Фабрика для создания экземпляра IParallelInvoker с автоматическим выводом generic-типов
    /// </summary>
    public static class ParallelInvokerFactory
    {
        public static IParallelInvoker
            Create<TChunk, TProcessedChunk>(ConveyorBase<TChunk, TProcessedChunk> conveyor) =>
            new ParallelInvoker<ConveyorBase<TChunk, TProcessedChunk>, TChunk, TProcessedChunk>(conveyor);
    }
}