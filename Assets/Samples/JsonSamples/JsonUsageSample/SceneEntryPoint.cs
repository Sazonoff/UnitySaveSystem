using UnityEngine;
using UnitySaveSystem.Saves.Json;
using Random = UnityEngine.Random;

namespace UnitySaveSystem.Saves.Samples.JsonMigrationSample.Usage
{
    public class SceneEntryPoint : MonoBehaviour
    {
        private ISavesSystem savesSystem;
        private SampleSave sampleSave1;
        private SampleSave sampleSave2;
        private SampleSave2 anotherSampleSave;

        private void Awake()
        {
            var assemblyWithSaves = typeof(SampleSave).Assembly;
            var jsonSaveProvider = new JsonSaveProvider();
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

            anotherSampleSave = savesSystem.GetSave<SampleSave2>();
            Debug.Log($"Another Sample save after load: {anotherSampleSave.SomeNumber}");
            anotherSampleSave.SomeNumber = Random.Range(0, 5000);
            anotherSampleSave.SetDirty();
            Debug.Log($"Another Sample save after change: {anotherSampleSave.SomeNumber}");
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