using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnitySaveSystem.Saves
{
    /// <summary>
    /// Checks only passed into ctor assemblies for Save Types
    /// </summary>
    public class SpecificAssembliesSaveTypesProvider : ISavesTypesProvider
    {
        private readonly Assembly[] assemblyToSearch;
        private Dictionary<Type, SaveData> saveDatas = new();
        private List<Type> saveTypes = new();
        private bool isLoaded;

        public SpecificAssembliesSaveTypesProvider(Assembly[] assemblyToSearch)
        {
            this.assemblyToSearch = assemblyToSearch;
        }

        public void Initialize()
        {
            if (isLoaded) return;
            LoadSaveTypes();
            GetAllSaveTypes();
            FillSaveData();
            isLoaded = true;
        }

        private void LoadSaveTypes()
        {
            foreach (var assembly in assemblyToSearch)
            {
                var foundedTypes = assembly.GetTypes()
                    .Where(p => p.IsClass && !p.IsAbstract && p.IsSubclassOf(typeof(Save)) &&
                                p.GetCustomAttribute<SaveAttribute>() != null);
                saveTypes.AddRange(foundedTypes);
            }
        }

        public IEnumerable<Type> GetAllSaveTypes()
        {
            return saveTypes;
        }

        private void FillSaveData()
        {
            foreach (var saveType in saveTypes)
            {
                var attribute = saveType.GetCustomAttribute<SaveAttribute>();
                var saveData = new SaveData(attribute.SaveName, attribute.SafeExtension, saveType);
                saveDatas.Add(saveType, saveData);
            }
        }

        public SaveData GetSaveData(Type saveType)
        {
            return saveDatas[saveType];
        }
    }
}