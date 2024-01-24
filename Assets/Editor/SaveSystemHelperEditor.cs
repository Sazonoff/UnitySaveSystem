using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sazonoff.CodeGenerator;
using UnityEditor;
using UnityEngine;

namespace UnitySaveSystem.Saves
{
    public class SaveSystemHelperEditor : MonoBehaviour
    {
        private const string PreviousPathForPreloadSave = "Sazonoff_PreviousPreloadClass";

        [MenuItem("Tools/SaveSystem/Open Persistent Data Folder")]
        public static void OpenSaveFolder()
        {
            var pathToSaveFolder =
                Path.Combine(Application.persistentDataPath);
            EditorUtility.RevealInFinder(pathToSaveFolder);
        }

        [MenuItem("Tools/SaveSystem/Delete Saves")]
        public static void DeleteSaves()
        {
            if (EditorUtility.DisplayDialog("Delete all user saves?", "Are you sure?", "Delete", "Cancel"))
            {
                var pathToSaveFolder =
                    Path.Combine(Application.persistentDataPath, SavesSystem.BaseSaveFolder);
                Directory.Delete(pathToSaveFolder, true);
            }
        }

        [MenuItem("Tools/SaveSystem/Generate Preload Save")]
        public static void GeneratePreloadSave()
        {
            var prevPath = PlayerPrefs.GetString(PreviousPathForPreloadSave, Application.dataPath);
            var pathToScript = EditorUtility.SaveFilePanel("Generate Preload Save Class Name", prevPath,
                "PreloadSave",
                "cs");
            if (string.IsNullOrEmpty(pathToScript))
            {
                return;
            }


            var allSaveTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(SaveFile)));
            HashSet<string> saveNamespaces = new();
            saveNamespaces.Add("UnitySaveSystem.Saves");
            foreach (var saveType in allSaveTypes)
            {
                if (!string.IsNullOrEmpty(saveType.Namespace))
                {
                    saveNamespaces.Add(saveType.Namespace);
                }
            }


            GeneratedClass preloadClass = new GeneratedClass("PreloadSavesHelper", null);
            var saveSystemField = new GeneratedField("savesSystem", "ISavesSystem", GeneratedAccessType.@private,
                isReadOnly: true);
            preloadClass.AddField(saveSystemField);
            foreach (var saveNamespace in saveNamespaces)
            {
                preloadClass.AddUsing(saveNamespace);
            }

            var constructor = new GeneratedMethod(GeneratedAccessType.@public, "PreloadSavesHelper", String.Empty);
            constructor.AddParameter(new GeneratedMethodParameter("savesSystem", "ISavesSystem"));
            constructor.AddBody("this.savesSystem = savesSystem;");
            preloadClass.AddMethod(constructor);

            var callMethod = new GeneratedMethod(GeneratedAccessType.@public, "PreloadSaves");
            foreach (var saveType in allSaveTypes)
            {
                callMethod.AddBody($"savesSystem.PreloadAllSavesOfType<{saveType.Name}>();");
            }

            preloadClass.AddMethod(callMethod);

            File.WriteAllText(pathToScript, preloadClass.ToCode());
            AssetDatabase.Refresh();
        }
    }
}