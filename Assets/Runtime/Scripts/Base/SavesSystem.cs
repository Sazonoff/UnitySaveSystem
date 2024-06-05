using System;
using System.Collections.Generic;
using System.Threading;

namespace UnitySaveSystem.Saves
{
    public class SavesSystem : ISavesSystem
    {
        public const string BaseSaveFolder = "Saves";

        private readonly ISavesTypesProvider savesTypesProvider;
        private readonly ISaveProvider saveProvider;
        private readonly ISaveToFileLogic saveToFileLogic;
        private readonly SaveSystemLogger logger;
        private bool isInitialized;

        private Dictionary<Type, SaveContainer> cachedSavesContainers = new();
        private HashSet<SaveContainer> dirtySaves = new();

        private Thread writingThread;
        private readonly AutoResetEvent waitHandler = new(false);

        private bool prevSaveInProgress;
        private int prevSaveCounter;
        private int saveCounter;
        public bool IsSaveInProgress { get; private set; }

        public event Action<bool> SaveInProgressChanged = delegate { };

        public SavesSystem(ISavesTypesProvider savesTypesProvider, ISaveProvider saveProvider,
            ISaveToFileLogic saveToFileLogic)
        {
            this.savesTypesProvider = savesTypesProvider;
            this.saveProvider = saveProvider;
            this.saveToFileLogic = saveToFileLogic;
            this.logger = new SaveSystemLogger();
        }

        public void Initialize(SaveSystemLogType logType)
        {
            logger.Initialize(logType);
            saveToFileLogic.SetLogger(logger);
            savesTypesProvider.Initialize();
            saveProvider.InjectDependencies(savesTypesProvider, logger, saveToFileLogic);
            saveProvider.Initialize();

            isInitialized = true;
            writingThread = new Thread(WriteChangedSaves) { IsBackground = true };
            writingThread.Start();
        }

        public void PreloadAllSavesOfType<T>() where T : Save
        {
            LoadContainer<T>();
        }

        public ISaveSystemLogger GetLogger()
        {
            return logger;
        }

        private void Uninitialize()
        {
            if (!isInitialized) return;
            SaveDirtyFiles();
            isInitialized = false;
            waitHandler.Set();
            writingThread.Join();
        }

        private SaveContainer LoadContainer<T>() where T : Save
        {
            if (!cachedSavesContainers.ContainsKey(typeof(T)))
            {
                var saves = (IEnumerable<T>)saveProvider.GetAllSavesOfType<T>();
                var container = new SaveContainer(typeof(T), saves);
                container.SomeSaveChanged += OnSaveContainerChanged;
                cachedSavesContainers.Add(typeof(T), container);
                return container;
            }

            return cachedSavesContainers[typeof(T)];
        }

        private Save CreateNewInstanceOfSave(int id, Type saveType)
        {
            logger.Log($"New save was created {saveType.Name} with ID {id}", SaveSystemLogType.Debug);
            var saveInstance = (Save)Activator.CreateInstance(saveType);
            saveInstance.Id = id;
            saveInstance.OnNewInstanceCreated();
            saveInstance.SetDirty();
            return saveInstance;
        }

        private void OnSaveContainerChanged(SaveContainer container)
        {
            dirtySaves.Add(container);
        }

        public T GetSave<T>(int id = 0) where T : Save
        {
            var container = LoadContainer<T>();
            if (container.HaveSaveWithId(id))
            {
                return (T)container.GetSaveById(id);
            }

            var newSave = CreateNewInstanceOfSave(id, typeof(T));
            container.AddNewSave(newSave);
            return (T)newSave;
        }

        public IEnumerable<T> GetSaves<T>() where T : Save
        {
            var container = LoadContainer<T>();
            return (IEnumerable<T>)container.GetAllSaves();
        }

        public void SaveDirtyFiles()
        {
            if (prevSaveInProgress != IsSaveInProgress)
            {
                SaveInProgressChanged.Invoke(IsSaveInProgress);
                prevSaveInProgress = IsSaveInProgress;
                prevSaveCounter = saveCounter;
            }
            else
            {
                if (prevSaveCounter != saveCounter)
                {
                    //Sometimes save is so fast that it starts and ends inside one main thread frame
                    //But I'm still wanna show notification to user so he can be sure that his progress was saved
                    //I think using lock or any thread sync methods for that case would be unnecessary overhead
                    //So I'm using that little trick with counter comparison
                    //Is is possible to have race condition here but it's just delaying notification to the next save cycle call
                    SaveInProgressChanged.Invoke(true);
                    SaveInProgressChanged.Invoke(false);
                    prevSaveCounter = saveCounter;
                }
            }

            if (dirtySaves.Count > 0)
            {
                logger.Log("SaveSystem was asked to save dirty files. Passing them to SaveProvider serialize",
                    SaveSystemLogType.Verbose);
                saveProvider.AddSavesToWrite(dirtySaves);
                foreach (var save in dirtySaves)
                {
                    save.ResetDirty();
                }

                dirtySaves.Clear();
                waitHandler.Set();
            }
        }

        private void WriteChangedSaves()
        {
            try
            {
                while (isInitialized)
                {
                    logger.Log("Writing thread waiting for signal", SaveSystemLogType.Verbose);
                    waitHandler.WaitOne();
                    bool requiredNotification = saveProvider.AnySaveRequiresNotification;
                    if (requiredNotification)
                    {
                        IsSaveInProgress = true;
                    }

                    logger.Log("Writing thread starting to write", SaveSystemLogType.Verbose);
                    saveProvider.WriteSaves();
                    if (requiredNotification)
                    {
                        ++saveCounter;
                        IsSaveInProgress = false;
                    }

                    logger.Log("Writing thread ending write", SaveSystemLogType.Verbose);
                }
            }
            catch (Exception e)
            {
                logger.Log(e);
            }
        }

        public void Dispose()
        {
            saveProvider.Dispose();
            Uninitialize();
        }
    }
}