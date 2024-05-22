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

        protected override void SerializeDirtySaves(IEnumerable<SaveContainer> containers)
        {
            foreach (var container in containers)
            {
                foreach (var save in container.GetAllSaves())
                {
                    var bson = BsonMapper.Global.ToDocument(save);
                    var saveName = SavesTypesProvider.GetSaveData(container.MySaveType).SaveName;
                    var bsonSave = new BsonSave(container.MySaveType, save.Id, bson, saveName, false);
                    tempList.Add(bsonSave);
                    Logger.Log($"Serializing save {saveName} with Type:{container.MySaveType}",
                        SaveSystemLogType.Verbose);
                }
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
                Logger.Log($"Writing to db {bsonSave.SaveName} with Type:{bsonSave.Type}", SaveSystemLogType.Verbose);
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

        protected override IEnumerable<Save> ReadSaves<T>(Type type, SaveData saveData)
        {
            Logger.Log($"Reading from db type: {type.FullName}", SaveSystemLogType.Verbose);
            var collection = db.GetCollection<T>(saveData.SaveName);
            var saves = collection.FindAll();
            return saves;
        }

        public override void Dispose()
        {
            db?.Dispose();
        }

        public override bool AnySaveRequiresNotification => bsonsToSave.Any(b => b.NotifyUserAboutSaving);
    }
}