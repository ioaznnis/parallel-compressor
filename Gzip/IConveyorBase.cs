using System.Collections.Generic;

namespace Gzip
{
    /// <summary>
    /// Интерфейс, реализующий набор методов, для возможности блочной обработки алгоритмов, в том числе и параллельно.
    /// </summary>
    /// <typeparam name="TChunk"></typeparam>
    /// <typeparam name="TProcessedChunk"></typeparam>
    public interface IConveyorBase<TChunk, TProcessedChunk>
    {
        /// <summary>
        /// Инициализация конвейера
        /// </summary>
        /// <returns>Каждый шаг отдает новую порцию данных</returns>
        IEnumerable<TChunk> Initialize();

        /// <summary>
        /// Метод обработки порции данных
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        TProcessedChunk Iterate(TChunk chunk);

        /// <summary>
        /// Метод, собирающий все данные в итоговый результат
        /// </summary>
        /// <param name="enumerable"></param>
        void Complete(IEnumerable<TProcessedChunk> enumerable);
    }
}