using System;

namespace UnitySaveSystem.Saves
{
    public struct SaveData
    {
        public string SaveName { get; private set; }
        public Type SaveType { get; private set; }
        public string SaveExtension { get; private set; }
        public bool NotifyUserAboutSaves { get; private set; }

        public SaveData(string saveName, string saveExtension, Type type, bool notifyUserAboutSaves)
        {
            SaveName = saveName;
            SaveType = type;
            SaveExtension = saveExtension;
            NotifyUserAboutSaves = notifyUserAboutSaves;
        }
    }
}