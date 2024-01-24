using System;
using System.Collections.Generic;

namespace UnitySaveSystem.Saves
{
    /// <summary>
    /// This class encapsulate writing and reading from saves
    /// </summary>
    public interface ISaveProvider
    {
        void InjectDependencies(ISavesTypesProvider savesTypesProvider, SaveSystemLogger logger);

        /// <summary>
        /// Method for initializing your SaveSystem. Here you should calculate the required paths / open connections to db e.t.c  
        /// </summary>
        void Initialize();

        /// <summary>
        /// Method for serializing dirty saves to the required format.
        /// The reason for it to be here is to avoid locking SaveFiles during work in the main thread / avoid serializing some part of the file while another part is being changed.
        /// We serialize saves here in the main thread and store them for another thread to write  
        /// </summary>
        /// <param name="dirtySaves"></param>
        void AddSavesToWrite(IEnumerable<SaveFile> dirtySaves);

        /// <summary>
        /// Here we write serialized data to disk/db/whatever. Called from writing thread.
        /// </summary>
        void WriteSaves();

        /// <summary>
        /// Here we read save from disk/db/whatever  
        /// </summary>
        /// <param name="id">Save id</param>
        /// <param name="type">Type of save</param>
        /// <param name="saveData">Meta data for save</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        SaveFile GetSave<T>(int id, Type type, SaveData saveData) where T : SaveFile;

        IEnumerable<SaveFile> GetAllSaves<T>() where T : SaveFile;

        void RegisterSaveTypeMigrationHandler<T>(ISaveMigrationHandler<T> migrationHandler) where T : SaveFile;


        /// <summary>
        /// Called from IDisposable.Dispose of SavesSystem.
        /// Use it for any unmanaged code - close connection to db/streams e.t.c.
        /// </summary>
        void Dispose();
    }
}