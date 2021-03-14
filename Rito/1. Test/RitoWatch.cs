using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

// 2021. 02. 19. 15:00
// 작성자 : Rito

namespace Rito
{
    public static class RitoWatch
    {
        private class Record
        {
            public string content;
            public long elapsedMS;
            public long elapsedTicks;
        }

        private static Stopwatch _sw;
        private static int _index;
        private static Dictionary<int, Record> _dictRecord;
        private static bool _isChecking;

        static RitoWatch()
        {
            _sw = new Stopwatch();
            _dictRecord = new Dictionary<int, Record>();
            _index = 0;
        }

        public static void Clear()
        {
            _dictRecord.Clear();
            _index = 0;
        }

        public static void BeginCheck(string content)
        {
            if (_isChecking) EndCheck();

            _dictRecord.Add(_index, new Record());
            _dictRecord[_index].content = content;

            _sw.Restart();
            _isChecking = true;
        }

        public static void EndCheck()
        {
            _sw.Stop();
            _dictRecord[_index].elapsedMS = _sw.ElapsedMilliseconds;
            _dictRecord[_index].elapsedTicks = _sw.ElapsedTicks;

            _index++;
            _isChecking = false;
        }

        public static void PrintLog(int index)
        {
            if(_isChecking) throw new StowatchStillRunningException();

            var record = _dictRecord[index];
            string msg = $"[{index}] {record.content}\n ms : {record.elapsedMS}, ticks : {record.elapsedTicks}";
            UnityEngine.Debug.Log(msg);
        }

        public static void PrintAllLogs()
        {
            for (int i = 0; i < _index; i++)
            {
                PrintLog(i);
            }
        }

        public static string GetAllLogString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < _index; i++)
            {
                var record = _dictRecord[i];
                string msg = $"[{i}] {record.content}\n ms : {record.elapsedMS}, ticks : {record.elapsedTicks}\n\n";
                sb.Append(msg);
            }

            return sb.ToString();
        }

        /***********************************************************************
        *                               Exception
        ***********************************************************************/
        #region .
        public class StopwatchAlreadyRunningException : System.Exception
        {
            public StopwatchAlreadyRunningException() : base("Stopwatch Already Running.") { }
        }

        public class StowatchStillRunningException : System.Exception
        {
            public StowatchStillRunningException() : base("Please Stop") { }
        }

        #endregion
    }
}