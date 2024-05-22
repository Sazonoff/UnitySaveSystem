using System;
using System.Collections.Generic;

namespace UnitySaveSystem.Saves
{
    public interface ISavesSystem : IDisposable
    {
        /// <summary>
        /// Initializes save system
        /// </summary>
        /// <param name="logType">what level of logs you want to see. NOTE: By default logs are completely stripped unless you define symbol: SAZONOFF_SAVESYSTEM_LOGSENABLED</param>
        void Initialize(SaveSystemLogType logType);

        void PreloadAllSavesOfType<T>() where T : Save;

        /// <summary>
        /// Get Inner Logger for controlling log flow
        /// </summary>
        /// <returns></returns>
        ISaveSystemLogger GetLogger();

        /// <summary>
        /// That callback will be called everytime saving(writing) started/finished
        /// </summary>
        event Action<bool> SaveInProgressChanged;

        /// <summary>
        /// Check if some saves is writing right now
        /// </summary>
        bool IsSaveInProgress { get; }

        /// <summary>
        /// Get save
        /// </summary>
        /// <param name="id">save id. 0 by default</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetSave<T>(int id = 0) where T : Save;

        /// <summary>
        /// Get All saves of type
        /// Useful for Save 'Slots' or in any other cases when you need to display all of the saves of type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> GetSaves<T>() where T : Save;

        /// <summary>
        /// Ask save system to get all dirty saves and push them to save provider for writing.
        /// Preferably to call once in LateUpdate
        /// </summary>
        void SaveDirtyFiles();
    }
}