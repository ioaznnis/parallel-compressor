using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Gzip
{
    /// <summary>
    /// Интерфейс для параллельного вычислителя <see cref="ConveyorBase{TChunk,TProcessedChunk}"/> для избавления от необходимости указывать generic аргументы
    /// </summary>
    public interface IParallelInvoker
    {
        void Invoke();
    }

    /// <summary>
    /// Основной класс для реализации многопоточности
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TChunk"></typeparam>
    /// <typeparam name="TProcessedChunk"></typeparam>
    public class ParallelInvoker<T, TChunk, TProcessedChunk> : IParallelInvoker
        where T : ConveyorBase<TChunk, TProcessedChunk>
    {
        private readonly T _conveyor;
        private readonly int _maxDegreeOfParallelism;

        private readonly Queue<TProcessedChunk> _queue = new Queue<TProcessedChunk>();

        private readonly SemaphoreSlim _semaphoreRead = new SemaphoreSlim(1,1);
        private readonly object _locker = new object();
        private bool _ended = false;
        private bool _canceled = false;

        private IEnumerator<Chunk<TChunk>> _enumerator;

        private long _currentBlock;

        private readonly ICollection<Exception> _exceptionCollection = new List<Exception>();

        public ParallelInvoker(T conveyor)
        {
            _conveyor = conveyor;
            _maxDegreeOfParallelism = Environment.ProcessorCount;
        }

        public void Invoke()
        {
            //i - int. Возможно стоит поменять на long
            _enumerator = _conveyor.Initialize().Select((chunk, i) => new Chunk<TChunk>(i, chunk)).GetEnumerator();
            _currentBlock = 0L;

            try
            {
                var events = new WaitHandle[_maxDegreeOfParallelism];
                for (var i = 0; i < _maxDegreeOfParallelism; i++)
                {
                    var autoResetEvent = new AutoResetEvent(false);

                    events[i] = autoResetEvent;

                    ThreadHelper.StartThread(Work, autoResetEvent, LogAction);
                }

                var eventWrite = new AutoResetEvent(false);
                ThreadHelper.StartThread(Complete, eventWrite, LogAction);

                WaitHandle.WaitAll(events);
                lock (_locker)
                {
                    _ended = true;
                    Monitor.PulseAll(_locker);
                }

                eventWrite.WaitOne();

                lock (_locker)
                {
                    if (_exceptionCollection.Any())
                    {
                        throw new AggregateException(_exceptionCollection);
                    }
                }
            }
            finally
            {
                _enumerator.Dispose();
            }
        }

        /// <summary>
        /// Сбор исключений
        /// </summary>
        /// <param name="exception"></param>
        private void LogAction(Exception exception)
        {
            lock (_locker)
            {
                _canceled = true;
                Monitor.PulseAll(_locker);

                _exceptionCollection.Add(exception);
            }
        }

        /// <summary>
        /// Основной метод обработки, вызываемый в нескольких потоках
        /// </summary>
        private void Work()
        {
            var chunk = Get();
            while (chunk != null)
            {
                var processedChunk = _conveyor.Iterate(chunk.Block);

                lock (_locker)
                {
                    if (_canceled) return;

                    //Плохое место, надо научиться определять нужный поток
                    while (_currentBlock != chunk.Id)
                    {
                        if (_canceled) return;
                        Monitor.Wait(_locker);
                    }

                    _queue.Enqueue(processedChunk);
                    Interlocked.Increment(ref _currentBlock);
                    Monitor.PulseAll(_locker);

                    //если у нас накопились блоки, то подождем, пока они не запишутся
                    while (_queue.Count > _maxDegreeOfParallelism)
                    {
                        if (_canceled) return;
                        Monitor.Wait(_locker);
                    }
                }

                chunk = Get();
            }
        }

        /// <summary>
        /// Получение очередного блока данных
        /// </summary>
        /// <returns></returns>
        private Chunk<TChunk> Get()
        {
            _semaphoreRead.Wait();
            try
            {
                return _enumerator.MoveNext() ? _enumerator.Current : null;
            }
            finally
            {
                _semaphoreRead.Release();
            }
        }

        /// <summary>
        /// Агрегирует результат работы "рабочих" потоков в итоговый результат/>
        /// </summary>
        private void Complete()
        {
            _conveyor.Complete(CollectChunk());
        }

        /// <summary>
        /// Собирает результат работ потоков в IEnumerable.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<TProcessedChunk> CollectChunk()
        {
            while (true)
            {
                lock (_locker)
                {
                    if (_canceled) yield break;

                    TProcessedChunk chunk;
                    while (!_queue.TryDequeue(out chunk))
                    {
                        if (_ended) yield break;
                        Monitor.Wait(_locker);
                        if (_canceled) yield break;
                    }

                    yield return chunk;
                    Monitor.PulseAll(_locker);
                }
            }
        }
    }
}