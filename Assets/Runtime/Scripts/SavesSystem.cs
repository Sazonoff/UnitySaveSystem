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
        private readonly SaveSystemLogger logger;
        private bool isInitialized;

        private Dictionary<Type, Dictionary<int, SaveFile>> cachedSaves = new();
        private HashSet<SaveFile> dirtySaves = new();

        private Thread writingThread;
        private readonly AutoResetEvent waitHandler = new(false);

        private bool prevSaveInProgress;
        private int prevSaveCounter;
        private int saveCounter;
        public bool IsSaveInProgress { get; private set; }
        public event Action<bool> SaveInProgressChanged = delegate { };

        public SavesSystem(ISavesTypesProvider savesTypesProvider, ISaveProvider saveProvider)
        {
            this.savesTypesProvider = savesTypesProvider;
            this.saveProvider = saveProvider;
            this.logger = new SaveSystemLogger();
        }

        public void Initialize(SaveSystemLogType logType)
        {
            logger.Initialize(logType);
            savesTypesProvider.Initialize();
            saveProvider.InjectDependencies(savesTypesProvider, logger);
            saveProvider.Initialize();

            isInitialized = true;
            writingThread = new Thread(WriteChangedSaves) { IsBackground = true };
            writingThread.Start();
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

        private SaveFile CreateNewInstanceOfSave(int id, Type saveType)
        {
            logger.Log($"New save was created {saveType.Name} with ID {id}", SaveSystemLogType.Debug);
            var saveInstance = (SaveFile)Activator.CreateInstance(saveType);
            saveInstance.Id = id;
            saveInstance.OnNewInstanceCreated();
            saveInstance.SaveChanged += OnSaveChanged;
            saveInstance.SetDirty();
            CacheSave(saveInstance);
            return saveInstance;
        }

        private void CacheSave(SaveFile saveFile)
        {
            logger.Log($"Storing save {saveFile.GetType().FullName} with ID {saveFile.Id} to cache",
                SaveSystemLogType.Debug);
            var type = saveFile.GetType();
            if (!cachedSaves.ContainsKey(type))
            {
                cachedSaves.Add(type, new Dictionary<int, SaveFile>());
            }

            cachedSaves[type].Add(saveFile.Id, saveFile);
        }

        private void OnSaveChanged(SaveFile saveFile)
        {
            dirtySaves.Add(saveFile);
        }

        public T GetSave<T>(int id = 0) where T : SaveFile
        {
            if (cachedSaves.ContainsKey(typeof(T)))
            {
                if (cachedSaves[typeof(T)].TryGetValue(id, out var data))
                {
                    logger.Log($"Returning save from cache {typeof(T).Name} with ID {id}", SaveSystemLogType.Debug);
                    return (T)data;
                }

                var loadedSave =
                    saveProvider.GetSave<T>(id, typeof(T), savesTypesProvider.GetSaveData(typeof(T)));
                if (loadedSave != null)
                {
                    if (loadedSave.IsDirty)
                    {
                        OnSaveChanged(loadedSave);
                    }

                    loadedSave.SaveChanged += OnSaveChanged;
                    CacheSave(loadedSave);
                    return (T)loadedSave;
                }

                var newSave = (T)CreateNewInstanceOfSave(id, typeof(T));
                return newSave;
            }
            else
            {
                var loadedSave =
                    saveProvider.GetSave<T>(id, typeof(T), savesTypesProvider.GetSaveData(typeof(T)));
                if (loadedSave != null)
                {
                    if (loadedSave.IsDirty)
                    {
                        OnSaveChanged(loadedSave);
                    }

                    loadedSave.SaveChanged += OnSaveChanged;
                    CacheSave(loadedSave);
                    return (T)loadedSave;
                }

                var newSave = (T)CreateNewInstanceOfSave(id, typeof(T));
                return newSave;
            }
        }

        public void PreloadAllSavesOfType<T>() where T : SaveFile
        {
            foreach (var save in saveProvider.GetAllSaves<T>())
            {
                if (save.IsDirty)
                {
                    OnSaveChanged(save);
                }

                save.SaveChanged += OnSaveChanged;
                CacheSave(save);
            }
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
                    //Sometimes save is so fast that you don't even notice(in main thread) IsSaveInProgress boolean was changed between frames
                    //But I'm still wanna show notification to user so he can be sure that his progress was saved
                    //I think using lock or any thread sync methods for that case would be completely overhead
                    //So i'm using that little trick with counter comparison
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