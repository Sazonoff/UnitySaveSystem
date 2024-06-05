using System.Collections.Generic;

namespace UnitySaveSystem.Saves
{
    /// <summary>
    /// This class encapsulate writing and reading from saves
    /// </summary>
    public interface ISaveProvider
    {
        void InjectDependencies(ISavesTypesProvider savesTypesProvider, SaveSystemLogger logger,
            ISaveToFileLogic saveToFileLogic);

        /// <summary>
        /// Method for initializing your SaveSystem. Here you should calculate the required paths / open connections to db e.t.c  
        /// </summary>
        void Initialize();

        /// <summary>
        /// Method for serializing dirty saves to the required format.
        /// The reason for it to be here is to avoid locking SaveFiles during work in the main thread / avoid serializing some part of the file while another part is being changed.
        /// We serialize saves here in the main thread and store them for another thread to write  
        /// </summary>
        /// <param name="dirtySaveContainers"></param>
        void AddSavesToWrite(IEnumerable<SaveContainer> dirtySaveContainers);

        /// <summary>
        /// Here we write serialized data to disk/db/whatever. Called from writing thread.
        /// </summary>
        void WriteSaves();

        /// <summary>
        /// Here we read save from disk/db/whatever  
        /// </summary>
        IEnumerable<Save> GetAllSavesOfType<T>() where T : Save;

        void RegisterSaveTypeMigrationHandler<T>(ISaveMigrationHandler<T> migrationHandler) where T : Save;


        /// <summary>
        /// Called from IDisposable.Dispose of SavesSystem.
        /// Use it for any unmanaged code - close connection to db/streams e.t.c.
        /// </summary>
        void Dispose();

        bool AnySaveRequiresNotification { get; }
    }
}