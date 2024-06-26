﻿using System;

namespace UnitySaveSystem.Saves.Json
{
    public class JsonSave
    {
        public Type Type { get; }
        public string Json { get; }
        public string SaveName { get; }
        public string SaveExtension { get; }
        public bool RequiresSaveNotificationToUser { get; }

        public JsonSave(Type type, string json, string saveName, string saveExtension,
            bool requiresSaveNotificationToUser)
        {
            Type = type;
            Json = json;
            SaveName = saveName;
            SaveExtension = saveExtension;
            RequiresSaveNotificationToUser = requiresSaveNotificationToUser;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
    }
}