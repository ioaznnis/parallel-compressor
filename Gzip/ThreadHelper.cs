using System;
using System.Threading;

namespace Gzip
{
    internal static class ThreadHelper
    {
        /// <summary>
        /// Метод, создающий и запускающий новый <see cref="Thread"/> со статической типизацией входных параметров потока
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="action"></param>
        /// <param name="param"></param>
        public static Thread StartThread<TT>(Action<TT> action, TT param)
        {
            var thread = new Thread(o => action((TT) o));
            thread.Start(param);
            return thread;
        }

        /// <summary>
        /// Метод, создающий и запускающий поток, оборачивая <param name="action"/>
        /// в обработку ошибок и вызывая объект сигнализации о завершении метода
        /// </summary>
        /// <param name="action"></param>
        /// <param name="autoResetEvent"></param>
        /// <param name="logAction"></param>
        /// <returns></returns>
        public static Thread StartThread(Action action, EventWaitHandle autoResetEvent, Action<Exception> logAction)
        {
            var thread = new Thread(() => ThreadMethod(action, autoResetEvent, logAction));
            thread.Start();
            return thread;
        }

        /// <summary>
        /// Декоратор основного метода, для однотипной обработки методов в потоках
        /// </summary>
        /// <param name="action"></param>
        /// <param name="autoResetEvent"></param>
        /// <param name="logAction"></param>
        private static void ThreadMethod(Action action, EventWaitHandle autoResetEvent, Action<Exception> logAction)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                logAction(e);
            }
            finally
            {
                autoResetEvent.Set();
            }
        }
    }
}