using System;
using System.Collections.Generic;
using System.Threading;

namespace Gzip
{
    /// <summary>
    /// Интерфейс для параллельного вычислителя <see cref="IConveyorBase{TChunk,TProcessedChunk}"/> для избавления от необходимости указывать generic аргументы
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
        where T : IConveyorBase<TChunk, TProcessedChunk>
    {
        private readonly T _conveyor;
        private readonly int _maxDegreeOfParallelism;

        private readonly Queue<TProcessedChunk> _queue = new Queue<TProcessedChunk>();

        private readonly SemaphoreSlim _semaphoreRead = new SemaphoreSlim(1, 1);
        private readonly object _locker = new object();
        private bool _ended = false;

        private IEnumerator<TChunk> _enumerator;

        public ParallelInvoker(T conveyor)
        {
            _conveyor = conveyor;
            _maxDegreeOfParallelism = Environment.ProcessorCount / 2;
        }

        public void Invoke()
        {
            _enumerator = _conveyor.Initialize().GetEnumerator();
            try
            {
                _autoResetEventWrite = new AutoResetEvent(false);
                var threadW = new Thread(Complete);


                _autoResetEvents = new WaitHandle[_maxDegreeOfParallelism];
                for (var i = 0; i < _maxDegreeOfParallelism; i++)
                {
                    var autoResetEvent = new AutoResetEvent(false);

                    _autoResetEvents[i] = autoResetEvent;

                    ThreadHelper.StartThread(Start, autoResetEvent);
                }

                threadW.Start();

                WaitHandle.WaitAll(_autoResetEvents);
                lock (_locker)
                {
                    _ended = true;
                    Monitor.PulseAll(_locker);
                }

                _autoResetEventWrite.WaitOne();
            }
            finally
            {
                _enumerator.Dispose();
            }
        }


        private AutoResetEvent _autoResetEventWrite;
        private WaitHandle[] _autoResetEvents;


        private void Start(AutoResetEvent autoResetEvent)
        {
            var (item1, chunk) = Get();
            do
            {
                var processedChunk = _conveyor.Iterate(chunk);

                lock (_locker)
                {
                    _queue.Enqueue(processedChunk);
                    Monitor.PulseAll(_locker);

                    while (_queue.Count > _maxDegreeOfParallelism)
                    {
                        Monitor.Wait(_locker);
                    }
                }

                (item1, chunk) = Get();
            } while (item1);

            autoResetEvent.Set();
        }


        private (bool b, TChunk enumeratorCurrent) Get()
        {
            _semaphoreRead.Wait();
            if (_enumerator.MoveNext())
            {
                var enumeratorCurrent = _enumerator.Current;
                _semaphoreRead.Release();
                return (true, enumeratorCurrent);
            }

            _semaphoreRead.Release();
            return (false, default);
        }

        /// <summary>
        /// Агрегирует результат работы "рабочих" потоков в итоговый результат/>
        /// </summary>
        private void Complete()
        {
            _conveyor.Complete(CollectChunk());

            _autoResetEventWrite.Set();
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
                    TProcessedChunk chunk;
                    while (!_queue.TryDequeue(out chunk))
                    {
                        Monitor.Wait(_locker);
                        if (_ended) yield break;
                    }

                    yield return chunk;
                    Monitor.PulseAll(_locker);
                }
            }
        }
    }
}