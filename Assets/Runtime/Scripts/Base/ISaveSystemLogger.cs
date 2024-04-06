using System;

namespace UnitySaveSystem.Saves
{
    public interface ISaveSystemLogger
    {
        public event Action<string, SaveSystemLogType> LogHappened;
        public void SetCallbackOnlyMode();
    }
}