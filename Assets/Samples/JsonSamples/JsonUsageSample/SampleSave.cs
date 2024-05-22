namespace UnitySaveSystem.Saves.Samples.JsonMigrationSample.Usage
{
    [Save("SampleSaveName")]
    public class SampleSave : Save
    {
        public int SomeNumber { get; set; }

        public SampleSave()
        {
        }
    }
}