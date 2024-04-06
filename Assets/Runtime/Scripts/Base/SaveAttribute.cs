using System;

namespace UnitySaveSystem.Saves
{
    public class SaveAttribute : Attribute
    {
        public string SaveName { get; }

        public SaveAttribute(string saveName)
        {
            SaveName = saveName;
        }
    }
}