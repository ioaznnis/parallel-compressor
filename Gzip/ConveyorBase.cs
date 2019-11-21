using System.Collections.Generic;
using System.Linq;

namespace Gzip
{
    /// <summary>
    /// Набор методов, для возможности блочной обработки алгоритмов, в том числе и параллельно.
    /// </summary>
    /// <typeparam name="TChunk"></typeparam>
    /// <typeparam name="TProcessedChunk"></typeparam>
    /// <remarks>Изначально это был интерфейс с попыткой использовать default interface methods, но ReSharper 2019.2.3 считает это ошибкой.
    /// https://youtrack.jetbrains.com/issue/RSRP-474628
    /// </remarks>
    public abstract class ConveyorBase<TChunk, TProcessedChunk>
    {
        /// <summary>
        /// Инициализация конвейера
        /// </summary>
        /// <returns>Каждый шаг отдает новую порцию данных</returns>
        public abstract IEnumerable<TChunk> Initialize();

        /// <summary>
        /// Метод обработки порции данных
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public abstract TProcessedChunk Iterate(TChunk chunk);

        /// <summary>
        /// Метод, собирающий все данные в итоговый результат
        /// </summary>
        /// <param name="enumerable"></param>
        public abstract void Complete(IEnumerable<TProcessedChunk> enumerable);

        /// <summary>
        /// Выполнить алгоритм в одном потоке
        /// </summary>
        public void SequentialRun() => Complete(Initialize().Select(Iterate));

        /// <summary>
        /// Выполнить алгоритм параллельно
        /// </summary>
        public void ParallelRun() => ParallelInvokerFactory.Create(this).Invoke();
    }
}