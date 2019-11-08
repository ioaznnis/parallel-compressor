namespace Gzip
{
    /// <summary>
    /// Фабрика для создания экземпляра IParallelInvoker с автоматическим выводом generic-типов
    /// </summary>
    public static class ParallelInvokerFactory
    {
        public static IParallelInvoker
            Create<TChunk, TProcessedChunk>(IConveyorBase<TChunk, TProcessedChunk> conveyor) =>
            new ParallelInvoker<IConveyorBase<TChunk, TProcessedChunk>, TChunk, TProcessedChunk>(conveyor);
    }
}