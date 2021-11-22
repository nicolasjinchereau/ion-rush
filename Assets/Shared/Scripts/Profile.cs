using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JsonFx;
using System.Linq;

public class EquipmentDictionaryConverter : JsonConverter
{
    public override bool CanConvert(Type t) {
        return t == typeof(Dictionary<EquipmentType, EquipmentState>);
    }
    
    public override object ReadJson(Type type, Dictionary<string, object> value) {
        return value.ToDictionary(
            kv => (EquipmentType)Enum.Parse(typeof(EquipmentType), kv.Key, true),
            kv => JsonReader.CoerceType<EquipmentState>(kv.Value)
        );
    }
    
    public override Dictionary<string, object> WriteJson(Type type, object value) {
        var dict = (Dictionary<EquipmentType, EquipmentState>)value;
        return dict.ToDictionary(d => d.Key.ToString(), d => (object)d.Value);
    }
}

public class LevelState
{
    public bool unlocked = false;
    public bool expertPassed = false;
    public int gearsCollected = 0;
    public int coinsCollected = 0;
    public int batteriesRemaining = 0;
    public float completionTime = 0;
    
    public LevelState(){}
    public LevelState(bool unlocked, bool expertPassed, int gearsCollected, int coinsCollected, int batteriesRemaining, float completionTime) {
        this.unlocked = unlocked;
        this.expertPassed = expertPassed;
        this.gearsCollected = gearsCollected;
        this.coinsCollected = coinsCollected;
        this.batteriesRemaining = batteriesRemaining;
        this.completionTime = completionTime;
    }
}

public class EquipmentState
{
    public bool unlocked = false;
    public bool equipped = false;
    
    public EquipmentState(){}
    public EquipmentState(bool unlocked, bool equipped) { 
        this.unlocked = unlocked;
        this.equipped = equipped;
    }
}

public class Profile
{
    public const int levelCount = 12;

    public string userUUID = new Guid().ToString();
    public bool showAds = true;
    public AppLanguage language = Strings.defaultLanguage;
    public bool musicEnabled = true;
    public bool soundEffectsEnabled = true;
    public float joystickLimit = 1.0f;
    public DifficultyLevel difficultyLevel = DifficultyLevel.Easy;
    public List<LevelState> levels = new List<LevelState>(new LevelState[levelCount]);
    public Dictionary<EquipmentType, EquipmentState> equipment = new Dictionary<EquipmentType, EquipmentState>();
    
    public static string UserUUID {
        get { return current.userUUID; }
        set { current.userUUID = value; }
    }
    
    public static bool ShowAds {
        get { return current.showAds; }
        set { current.showAds = value; }
    }
    
    public static AppLanguage Language {
        get { return current.language; }
        set { current.language = value; }
    }
    
    public static bool MusicEnabled {
        get { return current.musicEnabled; }
        set { current.musicEnabled = value; }
    }

    public static bool SoundEffectsEnabled {
        get { return current.soundEffectsEnabled; }
        set { current.soundEffectsEnabled = value; }
    }

    public static float JoystickLimit {
        get { return current.joystickLimit; }
        set { current.joystickLimit = value; }
    }

    public static DifficultyLevel DifficultyLevel {
        get { return current.difficultyLevel; }
        set { current.difficultyLevel = value; }
    }
    
    public static List<LevelState> Levels {
        get { return current.levels; }
    }
    
    public static Dictionary<EquipmentType, EquipmentState> Equipment {
        get { return current.equipment; }
    }
    
    public Profile()
    {
        levels[0] = new LevelState(true, false, 0, 0, 0, 0);
        for(int i = 1; i < levelCount; ++i)
            levels[i] = new LevelState(false, false, 0, 0, 0, 0);
        
        foreach(var eq in Util.EnumValues<EquipmentType>())
            equipment[eq] = new EquipmentState(false, false);
    }
    
    public void Serialize(string filename)
    {
        var settings = new JsonWriterSettings();
        settings.PrettyPrint = true;
        settings.AddTypeConverter(new EquipmentDictionaryConverter());
        string json = JsonWriter.Serialize(this, settings);
        File.WriteAllText(filename, json, Encoding.UTF8);
    }
    
    public static Profile Deserialize(string filename) {
        var settings = new JsonReaderSettings();
        settings.AddTypeConverter(new EquipmentDictionaryConverter());
        string json = File.ReadAllText(filename, Encoding.UTF8);
        return JsonReader.Deserialize<Profile>(json, settings);
    }
    
    private static Profile _current = null;
    public static Profile current {
        get { return _current ?? (_current = Load()); }
    }
    
    public static Profile Load()
    {
        Profile profile = null;
        
        {
            try {
                if(File.Exists(filename))
                    profile = Profile.Deserialize(filename);
            }
            catch(Exception ex){
                Debug.Log("Load Failed: " + ex.Message);
            }
        }
        
        if(profile == null)
        {
            try {
                if(File.Exists(backupFilename))
                    profile = Profile.Deserialize(backupFilename);
            }
            catch(Exception ex) {
                Debug.Log("Load Backup Failed: " + ex.Message);
            }
        }
        
        if(profile == null)
        {
            Debug.Log("Creating new profile");

            string docsPath = Util.documentsPath;
            
            if(!Directory.Exists(docsPath))
                Directory.CreateDirectory(docsPath);

            profile = new Profile();
            profile.Serialize(filename);
        }
        
        while(profile.levels.Count < levelCount)
            profile.levels.Add(new LevelState(false, false, 0, 0, 0, 0));

        while(profile.levels.Count > levelCount)
            profile.levels.RemoveAt(profile.levels.Count - 1);

        return profile;
    }
    
    public static void Save()
    {
        MakeBackup();
        string docsPath = Util.documentsPath;
        
        if(!Directory.Exists(docsPath))
            Directory.CreateDirectory(docsPath);
        
        current.Serialize(filename);
    }
    
    public static void MakeBackup()
    {
        try
        {
            if(File.Exists(filename))
            {
                if(File.Exists(backupFilename))
                    File.Delete(backupFilename);
                
                File.Copy(filename, backupFilename);
            }
        }
        catch(Exception ex)
        {
            Debug.Log("Failed to make backup: " + ex.Message);
        }
    }
    
    public static string filename {
        get { return Util.documentsPath + Path.DirectorySeparatorChar + "profile.dat"; }
    }
    
    public static string backupFilename {
        get { return Util.documentsPath + Path.DirectorySeparatorChar + "profile.bak"; }
    }
}
