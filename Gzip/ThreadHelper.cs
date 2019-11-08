using System;
using System.Threading;

namespace Gzip
{
    internal static class ThreadHelper
    {
        /// <summary>
        /// �����, ��������� � ����������� ����� <see cref="Thread"/> �� ����������� ���������� ������� ���������� ������
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
    }
}