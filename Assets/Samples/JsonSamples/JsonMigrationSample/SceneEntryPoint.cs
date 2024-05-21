using UnityEngine;
using UnitySaveSystem.Saves.Json;

namespace UnitySaveSystem.Saves.Samples.JsonMigrationSample.Migration
{
    public class SceneEntryPoint : MonoBehaviour
    {
        private ISavesSystem savesSystem;
        private SampleSave sampleSave1;
        private SampleSave sampleSave2;

        private void Awake()
        {
            var assemblyWithSaves = typeof(SampleSave).Assembly;
            var jsonSaveProvider = new JsonSaveProvider();
            var sampleSaveMigrationHandler = new DefaultSaveMigrationHandler<SampleSave>();
            sampleSaveMigrationHandler.AddMigrationRule(new SampleSaveMigration1());
            sampleSaveMigrationHandler.AddMigrationRule(new SampleSaveMigration2());
            //uncomment for test
            jsonSaveProvider.RegisterSaveTypeMigrationHandler<SampleSave>(sampleSaveMigrationHandler);
            savesSystem = new SavesSystem(new SpecificAssembliesSaveTypesProvider(new[] { assemblyWithSaves }),
                jsonSaveProvider);
            savesSystem.Initialize(SaveSystemLogType.Verbose);
            sampleSave1 = savesSystem.GetSave<SampleSave>();
            Debug.Log($"Sample save1 after load: {sampleSave1.SomeNumber}");
            sampleSave1.SomeNumber = Random.Range(0, 5000);
            sampleSave1.SetDirty();
            Debug.Log($"Sample save1 after change: {sampleSave1.SomeNumber}");

            sampleSave2 = savesSystem.GetSave<SampleSave>(1);
            Debug.Log($"Sample save2 after load: {sampleSave2.SomeNumber}");
            sampleSave2.SomeNumber = Random.Range(0, 5000);
            sampleSave2.SetDirty();
            Debug.Log($"Sample save2 after change: {sampleSave2.SomeNumber}");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                sampleSave1.SomeNumber = Random.Range(0, 5000);
                sampleSave1.SetDirty();
                Debug.Log($"Sample save1 after press: {sampleSave1.SomeNumber}");

                sampleSave2.SomeNumber = Random.Range(0, 5000);
                sampleSave2.SetDirty();
                Debug.Log($"Sample save2 after press: {sampleSave2.SomeNumber}");
            }
        }

        private void OnDestroy()
        {
            savesSystem.Dispose();
        }

        private void LateUpdate()
        {
            savesSystem.SaveDirtyFiles();
        }
    }
}