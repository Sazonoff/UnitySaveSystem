using System;
using System.Collections.Generic;

namespace UnitySaveSystem.Saves
{
    public abstract class SaveProvider : ISaveProvider
    {
        protected ISavesTypesProvider SavesTypesProvider;
        protected SaveSystemLogger Logger;
        protected ISaveToFileLogic SaveToFileLogic;
        protected readonly Dictionary<Type, ISaveMigrationWrapper> MigrationHandlers = new();

        public void InjectDependencies(ISavesTypesProvider savesTypesProvider, SaveSystemLogger logger,
            ISaveToFileLogic saveToFileLogic)
        {
            this.SavesTypesProvider = savesTypesProvider;
            this.Logger = logger;
            this.SaveToFileLogic = saveToFileLogic;
            foreach (var migrationHandler in MigrationHandlers.Values)
            {
                migrationHandler.InjectLogger(Logger);
            }
        }

        /// <summary>
        /// Method for initializing your SaveSystem. Here you should calculate the required paths / open connections to db e.t.c  
        /// </summary>
        public abstract void Initialize();

        public void AddSavesToWrite(IEnumerable<SaveContainer> containers)
        {
            foreach (var container in containers)
            {
                foreach (var save in container.GetAllSaves())
                {
                    if (MigrationHandlers.TryGetValue(save.GetType(), out var handler))
                    {
                        save.SavedMigrationId = handler.GetCurrentMigrationId();
                    }
                }
            }

            SerializeDirtySaves(containers);
        }

        protected abstract void SerializeDirtySaves(IEnumerable<SaveContainer> dirtySaves);

        public abstract void WriteSaves();

        public IEnumerable<Save> GetAllSavesOfType<T>() where T : Save
        {
            var saves = ReadSaves<T>(typeof(T), SavesTypesProvider.GetSaveData(typeof(T)));

            if (saves != null)
            {
                foreach (var save in saves)
                {
                    TryMigrate<T>(save);
                }
            }


            return saves;
        }

        protected abstract IEnumerable<Save> ReadSaves<T>(Type type, SaveData saveData) where T : Save;

        private void TryMigrate<T>(Save save) where T : Save
        {
            if (save == null) return;
            if (MigrationHandlers.TryGetValue(save.GetType(), out var handler))
            {
                var typedHandler = (ISaveMigrationHandler<T>)handler;
                typedHandler.TryMigrate((T)save);
            }
        }

        public void RegisterSaveTypeMigrationHandler<T>(ISaveMigrationHandler<T> migrationHandler) where T : Save
        {
            MigrationHandlers.Add(typeof(T), migrationHandler);
        }

        public abstract void Dispose();
        public abstract bool AnySaveRequiresNotification { get; }
    }
}