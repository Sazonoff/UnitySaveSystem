namespace UnitySaveSystem.Saves.Samples.JsonMigrationSample.Usage
{
    [Save("AnotherSampleSaveName", saveExtension: ".cfg")]
    public class SampleSave2 : Save
    {
        public int SomeNumber { get; set; }

        public SampleSave2()
        {
        }
    }
}