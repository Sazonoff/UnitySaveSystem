using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnitySaveSystem.Saves
{
    /// <summary>
    /// Checks all Domain Assemblies for Save Types
    /// </summary>
    public class AllLoadedAssembliesSaveTypesProvider : ISavesTypesProvider
    {
        private Dictionary<Type, SaveData> saveDatas = new();
        private List<Type> saveTypes = new();
        private bool isLoaded;

        public void Initialize()
        {
            if (isLoaded) return;
            GetAllSaveTypes();
            LoadAllSaveTypes();
            FillSaveData();
            isLoaded = true;
        }

        private void LoadAllSaveTypes()
        {
            var allDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in allDomainAssemblies)
            {
                var allSavesTypes = assembly.GetTypes()
                    .Where(p => p.IsClass && !p.IsAbstract && p.IsSubclassOf(typeof(Save)) &&
                                p.GetCustomAttribute<SaveAttribute>() != null);
                saveTypes.AddRange(allSavesTypes);
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