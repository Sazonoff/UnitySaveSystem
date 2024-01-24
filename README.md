# What is it?
Easy-to-use package for Unity in-game saves with:  
- Multiple saves of the same type
- Two built-in save options: Newtonsoft Json.Net / LiteDB
- Separate thread for writing saves
- Simple save migrations

# How to install?

## Installing base
This is just the base code for the save system.  
Check unity documentation on installing git packages: https://docs.unity3d.com/2019.3/Documentation/Manual/upm-git.html   

### Using package manager:
```
Window -> Package Manager -> '+' -> Add package from git url 
https://github.com/Sazonoff/UnityClassGenerator.git?path=Assets
And then
https://github.com/Sazonoff/UnitySaveSystem.git?path=Assets
```

### Using manifest.json
```
{
  "dependencies": {
    "com.sazonoff.unity-class-generator": "https://github.com/Sazonoff/UnityClassGenerator.git?path=Assets",
    "com.sazonoff.unitysavesystem-base": "https://github.com/Sazonoff/UnitySaveSystem.git?path=Assets"
}
```

### Installing SaveProvider
Base requires the installation of the so-called SaveProvider, which encapsulates the logic of writing/reading saves.  
Each save provider may contain their own additional installation instructions.  

Right now, there are two options to choose from:  
1. Newtonsoft JSON(JSON.NET) https://github.com/Sazonoff/UnitySaveSystemJsonProvider
2. LiteDB - saving in local db https://github.com/Sazonoff/UnitySaveSystemLiteDBProvider


# How to use
- Create a new instance of SaveSystem
- SaveSystem.Initialize(...) it
- Call SaveSystem.SaveDirtyFiles() as often as you want. I prefer to call it every LateUpdate() 
- Use SaveSystem.GetSave(int id) to get the required save
- After any change to save - call save.SetDirty()

# How do I make my own save?
- Create your save class.
- Inherit SaveFile.
- Add SaveAttribute to the class definition.
- Add default empty constructor.
- If you change some data call SetDirty() That's pretty much it.
Example:   

```
[Save("ExampleSaveName")]
public class ExampleSave : SaveFile
{

    public int SomeData { get; set; }
	
	public ExampleSave()
    {
    }
    
    public void ChangeData(int newValue)
    {
        SomeData = newValue;
        SetDirty();
    }
}
```

  
Look into Samples for more examples.   

> [!WARNING]
> Class structure can differ based on the save provider you're using. For example, JSON may require public setters/[JsonProperty] attribute. DB may not support all .Net types.

# Save migrations
Most of Save providers(and their formats) have their own migration differences.  
For example: Just adding new field with default value to json don't need any complex migration logic.  
  
But in some other cases you can register save migration handler.  
``` 
var mySaveTypeMigrationHandler = new DefaultSaveMigrationHandler<SampleSave>(); //creating migration handler
var migrationRule1 = new MySaveMigrationRule1(); //creating migration rule1
var migrationRule2 = new MySaveMigrationRule2(); //creating migration rule2
...
mySaveTypeMigrationHandler.AddMigrationRule(migrationRule1) //adding migration rule to handler
mySaveTypeMigrationHandler.AddMigrationRule(migrationRule2) //adding migration rule to handler
...
ISaveProvider.RegisterSaveTypeMigrationHandler<MySaveType>(mySaveTypeMigrationHandler);  //register handler in SaveProvider
```
There should be only one registered ISaveMigrationHandler for a save type and any amount of ISaveMigrationRule in it(but every rule should have unique Id).

SaveMigrationRule
Example:
```
 public class SampleSaveMigration1 : DefaultSaveMigrationRule<SampleSave>
    {
        public override int Id => 1; // Increase it for every new migration

        public override void Migrate(SampleSave save) //here we do any migration changes
        {
            save.SomeNumberAnother = save.SomeNumber;
            save.SetDirty();
        }
    }
```

# Can I implement my own save logic?
Sure. You need to implement your own ISaveProvider(or SaveProvider which has some basic migration logic in it) and pass it to SavesSystem ctor.

## My own migration logic?
In that case your save provider can't be inherited from SaveProvider. You should use ISaveProvider and 

# Logging
First of all, you need to add a scripting define symbol: https://docs.unity3d.com/Manual/CustomScriptingSymbols.html  
```SAZONOFF_SAVESYSTEM_LOGSENABLED```

Without it all log calls are completely stripped from code (so there are no memory allocations for log strings).   
After that, you pass  SaveSystemLogType enum to ISaveSystem.Initialize method with required level of logs.   
- ErrorsOnly - Only exceptions
- Debug: exceptions and a few debug logs
- Verbose - All the above and a lot of additional information

### What if I have my own log system?
In that case, you can call ISavesSystem.GetLogger() to get ISaveSystemLogger and subscribe to SaveSystemLogger.LogHappened event and handle logs by your code.   
But ISaveSystemLogger will continue to Debug.Log logs. You can change it by calling ISaveSystemLogger.SetCallbackOnlyMode();   

# Is it single thread?
Nope. You should always change saves in unity main thread. But after that, the system will use another thread for writing them (to disk or DB)  

# How can I preload all saves(and cache inside SaveSystem) on the start of the game?
Call ISavesSystem.PreloadAllSavesOfType<T>() for every Save Type after ISavesSystem.Initialize(...)  
You can also use code generation:  
```Tools -> Save System -> Generate Preload Save```
That will generate PreloadSavesHelper class with PreloadSaves() method with every call for your saves.  
Create it and call the method to preload all saves.  

# Can I disable Editor Scripts?
Yeah. Add a scripting define symbol
```SAZONOFF_SAVESYSTEM_DISABLEEDITOR```