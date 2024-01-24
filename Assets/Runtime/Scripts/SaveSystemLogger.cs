using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace UnitySaveSystem.Saves
{
    public class SaveSystemLogger : ISaveSystemLogger
    {
        private SaveSystemLogType currentLogType;
        private bool callbackOnly;
        public event Action<string, SaveSystemLogType> LogHappened = delegate { };

        public void SetCallbackOnlyMode()
        {
            callbackOnly = true;
        }

        [Conditional("SAZONOFF_SAVESYSTEM_LOGSENABLED")]
        public void Initialize(SaveSystemLogType logType)
        {
            currentLogType = logType;
        }

        [Conditional("SAZONOFF_SAVESYSTEM_LOGSENABLED")]
        public void Log(string msg, SaveSystemLogType logType)
        {
            if (logType > currentLogType)
            {
                return;
            }

            if (!callbackOnly)
            {
                switch (logType)
                {
                    case SaveSystemLogType.ErrorsOnly:
                        Debug.LogError(msg);
                        break;
                    case SaveSystemLogType.Debug:
                    case SaveSystemLogType.Verbose:
                    default:
                        Debug.Log(msg);
                        break;
                }
            }

            LogHappened?.Invoke(msg, logType);
        }

        [Conditional("SAZONOFF_SAVESYSTEM_LOGSENABLED")]
        public void Log(Exception exception)
        {
            Log(exception.Message + exception.StackTrace, SaveSystemLogType.ErrorsOnly);
        }
    }
}