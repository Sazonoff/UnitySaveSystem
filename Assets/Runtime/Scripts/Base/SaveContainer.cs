using System;
using System.Collections;
using System.Collections.Generic;

namespace UnitySaveSystem.Saves
{
    public class SaveContainer
    {
        public Type MySaveType { get; private set; }

        private Dictionary<int, Save> saves = new();
        public event Action<SaveContainer> SomeSaveChanged = delegate { };

        public SaveContainer(Type type, IEnumerable<Save> saves)
        {
            MySaveType = type;
            if (saves != null)
            {
                foreach (var s in saves)
                {
                    AddNewSave(s);
                }
            }
        }

        public bool HaveSaveWithId(int id)
        {
            return saves.ContainsKey(id);
        }

        public Save GetSaveById(int id)
        {
            return saves[id];
        }

        public IEnumerable<Save> GetAllSaves()
        {
            return saves.Values;
        }

        public void AddNewSave(Save save)
        {
            saves.Add(save.Id, save);
            save.SaveChanged += OnSomeOfMySavesChanged;
        }

        private void OnSomeOfMySavesChanged(Save save)
        {
            SomeSaveChanged.Invoke(this);
        }

        public void ResetDirty()
        {
            foreach (var save in saves.Values)
            {
                save.ResetDirty();
            }
        }
    }
}