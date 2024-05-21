namespace UnitySaveSystem.Saves.Samples.JsonMigrationSample.Migration
{
    [Save("SampleSaveName")]
    public class SampleSave : SaveFile
    {
        public int SomeNumber { get; set; }
        public int SomeNumberAnother { get; set; }

        public SampleSave()
        {
        }
    }
}