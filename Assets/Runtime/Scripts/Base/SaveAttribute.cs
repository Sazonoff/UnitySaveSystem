using System;
using JetBrains.Annotations;

namespace UnitySaveSystem.Saves
{
    public class SaveAttribute : Attribute
    {
        public string SaveName { get; }
        public string SafeExtension { get; } = ".save";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveName">Name for the save file. Should be unique</param>
        /// <param name="saveExtension">
        /// Custom extension for your save.
        /// Format: .extension
        /// Leave it null for default .save extension
        /// </param>
        public SaveAttribute(string saveName, [CanBeNull] string saveExtension = null)
        {
            SaveName = saveName;
            if (saveExtension != null)
            {
                SafeExtension = saveExtension;
            }
        }
    }
}