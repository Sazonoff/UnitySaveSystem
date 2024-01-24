using System;
using System.Collections.Generic;

namespace UnitySaveSystem.Saves
{
    public abstract class SaveProvider : ISaveProvider
    {
        protected ISavesTypesProvider SavesTypesProvider;
        protected SaveSystemLogger Logger;
        protected readonly Dictionary<Type, ISaveMigrationWrapper> MigrationHandlers = new();

        public void InjectDependencies(ISavesTypesProvider savesTypesProvider, SaveSystemLogger logger)
        {
            this.SavesTypesProvider = savesTypesProvider;
            this.Logger = logger;
            foreach (var migrationHandler in MigrationHandlers.Values)
            {
                migrationHandler.InjectLogger(Logger);
            }
        }

        /// <summary>
        /// Method for initializing your SaveSystem. Here you should calculate the required paths / open connections to db e.t.c  
        /// </summary>
        public abstract void Initialize();

        public void AddSavesToWrite(IEnumerable<SaveFile> dirtySaves)
        {
            foreach (var dirtySave in dirtySaves)
            {
                if (MigrationHandlers.TryGetValue(dirtySave.GetType(), out var handler))
                {
                    dirtySave.SavedMigrationId = handler.GetCurrentMigrationId();
                }
            }

            SerializeDirtySaves(dirtySaves);
        }

        protected abstract void SerializeDirtySaves(IEnumerable<SaveFile> dirtySaves);

        public abstract void WriteSaves();

        public SaveFile GetSave<T>(int id, Type type, SaveData saveData) where T : SaveFile
        {
            var saveFile = ReadSave<T>(id, type, saveData);
            if (saveFile != null)
            {
                TryMigrate<T>(saveFile);
            }

            return saveFile;
        }

        public abstract IEnumerable<SaveFile> GetAllSaves<T>() where T : SaveFile;

        private void TryMigrate<T>(SaveFile saveFile) where T : SaveFile
        {
            if (saveFile == null) return;
            if (MigrationHandlers.TryGetValue(saveFile.GetType(), out var handler))
            {
                var typedHandler = (ISaveMigrationHandler<T>)handler;
                typedHandler.TryMigrate((T)saveFile);
            }
        }

        protected abstract SaveFile ReadSave<T>(int id, Type type, SaveData saveData) where T : SaveFile;

        public void RegisterSaveTypeMigrationHandler<T>(ISaveMigrationHandler<T> migrationHandler) where T : SaveFile
        {
            MigrationHandlers.Add(typeof(T), migrationHandler);
        }

        public abstract void Dispose();
    }
}