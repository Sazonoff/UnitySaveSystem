using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace UnitySaveSystem.Saves.Json
{
    public class JsonSaveProvider : SaveProvider
    {
        private const string SaveFileExtension = ".save";
        private string pathToSaveFolder;
        private List<JsonSave> tempList = new();
        private List<JsonSave> jsonsToSave = new();
        private List<JsonSave> jsonsToSaveTemp = new();
        private readonly object lockObject = new();

        public override void Initialize()
        {
            pathToSaveFolder = Path.Combine(Application.persistentDataPath, SavesSystem.BaseSaveFolder);
            Logger.Log($"Directory for saves {pathToSaveFolder}", SaveSystemLogType.Verbose);

            if (!Directory.Exists(pathToSaveFolder))
            {
                Directory.CreateDirectory(pathToSaveFolder);
                Logger.Log($"Creating new directory for saves {pathToSaveFolder}", SaveSystemLogType.Verbose);
            }
        }

        protected override void SerializeDirtySaves(IEnumerable<SaveFile> dirtySaves)
        {
            foreach (var save in dirtySaves)
            {
                var jsonString = JsonConvert.SerializeObject(save);
                var saveName = SavesTypesProvider.GetSaveData(save.GetType()).SaveName;
                var jsonSave = new JsonSave(save.GetType(), save.Id, jsonString, saveName, save.NotifyUserAboutSaving);
                Logger.Log($"Save added for writing {jsonSave.SaveName} with ID:{jsonSave.Id}",
                    SaveSystemLogType.Verbose);
                tempList.Add(jsonSave);
            }

            lock (lockObject)
            {
                foreach (var save in tempList)
                {
                    var index = jsonsToSave.FindIndex(s => s.Type == save.Type && s.Id == save.Id);
                    if (index != -1)
                    {
                        jsonsToSave[index] = save;
                    }
                    else
                    {
                        jsonsToSave.Add(save);
                    }
                }
            }

            tempList.Clear();
        }

        public override void WriteSaves()
        {
            lock (lockObject)
            {
                (jsonsToSave, jsonsToSaveTemp) = (jsonsToSaveTemp, jsonsToSave);
            }

            foreach (var jsonSave in jsonsToSaveTemp)
            {
                WriteSave(jsonSave);
            }

            jsonsToSaveTemp.Clear();
        }

        private void WriteSave(JsonSave jsonSave)
        {
            try
            {
                var fileName = jsonSave.SaveName + jsonSave.Id + SaveFileExtension;
                var pathToFile = Path.Combine(pathToSaveFolder, fileName);
                Logger.Log($"Writing {jsonSave.SaveName} with ID:{jsonSave.Id}", SaveSystemLogType.Verbose);
                File.WriteAllText(pathToFile, jsonSave.Json);
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        protected override SaveFile ReadSave<T>(int id, Type type, SaveData saveData)
        {
            var fileName = saveData.SaveName + id + SaveFileExtension;
            var pathToFile = Path.Combine(pathToSaveFolder, fileName);
            if (File.Exists(pathToFile))
            {
                Logger.Log($"Reading {fileName} as {type.FullName}", SaveSystemLogType.Verbose);
                var fileText = File.ReadAllText(pathToFile);
                var save = JsonConvert.DeserializeObject(fileText, type);
                return (T)save;
            }

            Logger.Log($"Reading cancelled {fileName} not exist", SaveSystemLogType.Verbose);
            return null;
        }

        public override void Dispose()
        {
        }

        public override bool AnySaveRequiresNotification => jsonsToSave.Any(s => s.RequiresSaveNotificationToUser);

        public override IEnumerable<SaveFile> GetAllSaves<T>()
        {
            List<SaveFile> loadedSafe = new List<SaveFile>();
            var saveFile = GetSave<T>(0, typeof(T), SavesTypesProvider.GetSaveData(typeof(T)));
            if (saveFile != null)
            {
                loadedSafe.Add(saveFile);
            }

            return loadedSafe;
        }
    }
}