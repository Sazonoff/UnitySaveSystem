using System;
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
        private const string GitHubPage = "https://github.com/Sazonoff/UnitySaveSystem";

        public static void OpenSaveFolder()
        {
            var pathToSaveFolder =
                Path.Combine(Application.persistentDataPath);
            EditorUtility.RevealInFinder(pathToSaveFolder);
        }

        public static void DeleteSaves()
        {
            if (EditorUtility.DisplayDialog("Delete all user saves?", "Are you sure?", "Delete", "Cancel"))
            {
                var pathToSaveFolder =
                    Path.Combine(Application.persistentDataPath, SavesSystem.BaseSaveFolder);
                Directory.Delete(pathToSaveFolder, true);
            }
        }

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
                .Where(type => type.IsSubclassOf(typeof(Save)));

            GeneratedClass preloadClass = new GeneratedClass("PreloadSavesHelper", null);
            var saveSystemField = new GeneratedField("savesSystem", "ISavesSystem", GeneratedAccessType.@private,
                isReadOnly: true);
            preloadClass.AddField(saveSystemField);
            preloadClass.AddUsing("UnitySaveSystem.Saves");

            var constructor = new GeneratedMethod(GeneratedAccessType.@public, "PreloadSavesHelper", String.Empty);
            constructor.AddParameter(new GeneratedMethodParameter("savesSystem", "ISavesSystem"));
            constructor.AddBody("this.savesSystem = savesSystem;");
            preloadClass.AddMethod(constructor);

            var callMethod = new GeneratedMethod(GeneratedAccessType.@public, "PreloadSaves");
            foreach (var saveType in allSaveTypes)
            {
                callMethod.AddBody($"savesSystem.PreloadAllSavesOfType<{saveType.FullName}>();");
            }

            preloadClass.AddMethod(callMethod);

            File.WriteAllText(pathToScript, preloadClass.ToCode());
            AssetDatabase.Refresh();
        }

        public static void OpenGitHubPage()
        {
            Application.OpenURL(GitHubPage);
        }
    }
}