namespace UnitySaveSystem.Saves.Samples.JsonMigrationSample.Usage
{
    [Save("SampleSaveName")]
    public class SampleSave : SaveFile
    {
        public int SomeNumber { get; set; }

        public SampleSave()
        {
        }
    }
}