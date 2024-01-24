using System;
using System.Collections.Generic;

namespace UnitySaveSystem.Saves
{
    /// <summary>
    /// This class collects and stores all save types and their data
    /// </summary>
    public interface ISavesTypesProvider
    {
        /// <summary>
        /// Collect and store Save Types
        /// </summary>
        void Initialize();

        IEnumerable<Type> GetAllSaveTypes();
        SaveData GetSaveData(Type saveType);
    }
}