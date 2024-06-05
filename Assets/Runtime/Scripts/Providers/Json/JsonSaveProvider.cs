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
        private string pathToSaveFolder;
        private List<JsonSave> tempList = new();
        private List<JsonSave> jsonsToSave = new();
        private List<JsonSave> jsonsToSaveTemp = new();
        private readonly object lockObject = new();

        public override void Initialize()
        {
            pathToSaveFolder = Path.Combine(Application.persistentDataPath, SavesSystem.BaseSaveFolder);
            Logger.Log($"Directory for saves {pathToSaveFolder}", SaveSystemLogType.Verbose);

            if (!SaveToFileLogic.DirectoryExist(pathToSaveFolder))
            {
                SaveToFileLogic.CreateDirectory(pathToSaveFolder);
                Logger.Log($"Creating new directory for saves {pathToSaveFolder}", SaveSystemLogType.Verbose);
            }
        }

        protected override void SerializeDirtySaves(IEnumerable<SaveContainer> containers)
        {
            foreach (var container in containers)
            {
                var type = container.MySaveType;
                var saveData = SavesTypesProvider.GetSaveData(type);
                var jsonString = JsonConvert.SerializeObject(container.GetAllSaves());
                var saveName = saveData.SaveName;
                var saveExtension = saveData.SaveExtension;

                var jsonSave = new JsonSave(type, jsonString, saveName, saveExtension, saveData.NotifyUserAboutSaves);
                Logger.Log($"Saving container {jsonSave.SaveName} with type:{jsonSave.Type}",
                    SaveSystemLogType.Verbose);
                tempList.Add(jsonSave);
            }

            lock (lockObject)
            {
                foreach (var save in tempList)
                {
                    var index = jsonsToSave.FindIndex(s => s.Type == save.Type);
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
                var fileName = String.Concat(jsonSave.SaveName, jsonSave.SaveExtension);
                var pathToFile = Path.Combine(pathToSaveFolder, fileName);
                Logger.Log($"Writing container {jsonSave.SaveName} with Type:{jsonSave.Type}",
                    SaveSystemLogType.Verbose);
                SaveToFileLogic.WriteContentToFile(pathToFile, jsonSave.Json);
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        protected override IEnumerable<Save> ReadSaves<T>(Type type, SaveData saveData)
        {
            var arrayType = type.MakeArrayType();
            var fileName = String.Concat(saveData.SaveName, saveData.SaveExtension);
            var pathToFile = Path.Combine(pathToSaveFolder, fileName);
            if (SaveToFileLogic.IsFileExist(pathToFile))
            {
                Logger.Log($"Reading {fileName} as container for {type.FullName}", SaveSystemLogType.Verbose);
                var fileText = SaveToFileLogic.ReadTextFromFile(pathToFile);
                var save = JsonConvert.DeserializeObject(fileText, arrayType);
                return (T[])save;
            }

            Logger.Log($"Reading cancelled {fileName} not exist", SaveSystemLogType.Verbose);
            return null;
        }

        public override void Dispose()
        {
        }

        public override bool AnySaveRequiresNotification => jsonsToSave.Any(s => s.RequiresSaveNotificationToUser);
    }
}