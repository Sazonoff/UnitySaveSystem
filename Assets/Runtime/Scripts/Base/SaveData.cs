using System;

namespace UnitySaveSystem.Saves
{
    public struct SaveData
    {
        public string SaveName { get; private set; }
        public Type SaveType { get; private set; }
        public string SaveExtension { get; private set; }

        public SaveData(string saveName, string saveExtension, Type type)
        {
            SaveName = saveName;
            SaveType = type;
            SaveExtension = saveExtension;
        }
    }
}