using System;

namespace UnitySaveSystem.Saves
{
    public struct SaveData
    {
        public string SaveName { get; private set; }
        public Type SaveType { get; private set; }

        public SaveData(string saveName, Type type)
        {
            SaveName = saveName;
            SaveType = type;
        }
    }
}