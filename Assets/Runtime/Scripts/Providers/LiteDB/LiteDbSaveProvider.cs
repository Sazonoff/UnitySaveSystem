using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using UnityEngine;

namespace UnitySaveSystem.Saves.LiteDB
{
    public class LiteDbSaveProvider : SaveProvider
    {
        private const string DatabaseName = "PlayerSave.db";
        private LiteDatabase db;
        private string pathToSaveFolder;

        private List<BsonSave> tempList = new();
        private List<BsonSave> bsonsToSave = new();
        private List<BsonSave> bsonsToSaveTemp = new();
        private readonly object lockObject = new();

        public override void Initialize()
        {
            pathToSaveFolder = Path.Combine(Application.persistentDataPath, SavesSystem.BaseSaveFolder);
            if (!Directory.Exists(pathToSaveFolder))
            {
                Logger.Log($"Creating directory for saves {pathToSaveFolder}", SaveSystemLogType.Debug);
                Directory.CreateDirectory(pathToSaveFolder);
            }

            var pathToDb = Path.Combine(pathToSaveFolder, DatabaseName);
            Logger.Log($"Opening connection to db {pathToDb}", SaveSystemLogType.Verbose);
            db = new LiteDatabase(pathToDb, new BsonMapper());
        }

        protected override void SerializeDirtySaves(IEnumerable<SaveFile> dirtySaves)
        {
            foreach (var save in dirtySaves)
            {
                var bson = BsonMapper.Global.ToDocument(save);
                var saveName = SavesTypesProvider.GetSaveData(save.GetType()).SaveName;
                var bsonSave = new BsonSave(save.GetType(), save.Id, bson, saveName, save.NotifyUserAboutSaving);
                tempList.Add(bsonSave);
                Logger.Log($"Serializing {saveName} with ID:{save.Id}", SaveSystemLogType.Verbose);
            }

            lock (lockObject)
            {
                foreach (var bsonSave in tempList)
                {
                    var index = bsonsToSave.FindIndex(s => s.Type == bsonSave.Type && s.Id == bsonSave.Id);
                    if (index != -1)
                    {
                        bsonsToSave[index] = bsonSave;
                    }
                    else
                    {
                        bsonsToSave.Add(bsonSave);
                    }
                }
            }

            tempList.Clear();
        }

        public override void WriteSaves()
        {
            lock (lockObject)
            {
                (bsonsToSave, bsonsToSaveTemp) = (bsonsToSaveTemp, bsonsToSave);
            }

            foreach (var bsonSave in bsonsToSaveTemp)
            {
                Logger.Log($"Writing to db {bsonSave.SaveName} with ID:{bsonSave.Id}", SaveSystemLogType.Verbose);
                WriteSave(bsonSave);
            }

            bsonsToSaveTemp.Clear();
        }

        private void WriteSave(BsonSave bsonSave)
        {
            var collection = db.GetCollection(bsonSave.SaveName);
            var found = collection.FindById(bsonSave.Id);
            if (found != null)
            {
                collection.Update(bsonSave.Id, bsonSave.Bson);
            }
            else
            {
                collection.Insert(bsonSave.Id, bsonSave.Bson);
            }
        }

        protected override SaveFile ReadSave<T>(int id, Type type, SaveData saveData)
        {
            Logger.Log($"Reading from db type: {type.FullName} with ID:{id}", SaveSystemLogType.Verbose);
            var collection = db.GetCollection<T>(saveData.SaveName);
            var save = collection.FindById(id);
            return save;
        }

        public override IEnumerable<SaveFile> GetAllSaves<T>()
        {
            List<SaveFile> loadedSafe = new List<SaveFile>();
            foreach (var saveType in SavesTypesProvider.GetAllSaveTypes())
            {
                var collection = db.GetCollection<SaveFile>(SavesTypesProvider.GetSaveData(saveType).SaveName);
                var allSavesOfType = collection.FindAll();
                loadedSafe.AddRange(allSavesOfType);
            }

            return loadedSafe;
        }

        public override void Dispose()
        {
            db?.Dispose();
        }

        public override bool AnySaveRequiresNotification => bsonsToSave.Any(b => b.NotifyUserAboutSaving);
    }
}