namespace BuildAnywhere
{
    public class Settings
    {
        [JsonIgnore]
        public KeyCode _ButtonIgnoreBuildArea;
        public string ButtonIgnoreBuildArea = KeyCode.C.ToString();
        [JsonIgnore]
        public KeyCode _ButtonIgnoreBuildOverlap;
        public string ButtonIgnoreBuildOverlap = KeyCode.V.ToString();

        public bool Hotkeys = true;//Handles TeleportStone, TeleportMouse
        [JsonIgnore]
        public KeyCode _TeleportStone;
        public string TeleportStone = KeyCode.H.ToString();
        [JsonIgnore]
        public KeyCode _TeleportMouse;
        public string TeleportMouse = KeyCode.G.ToString();

        //public int BuildModeExpandView = 100;

        public bool FishingAlwaysSuccess = true;
        public bool FishAlwaysBiting = true;
        public bool FishPullAutomatically = true;
        public bool FishNoDurabiliyLoss = true;

        public bool AllObjectsNoDurability = true;
        public bool AutopsyNoConfirm = true;
        public bool AutoDropIntoStorage = true;
        public bool AutoDropIntoStoragePlayerStacks = false;//experimental
        public bool AutoDropIntoStoragePlayerAll = false;//experimental
        public bool ZombieDropTechOrbs = true;
        public bool PGB_global = true;//affects the next 3 as well
        public bool PGB_LinkStorageWellPump = true;
        public bool PGB_MoreZombieRecipies = true;
        public bool PGB__AllBuildingsAtAllBlueprintDesks = false;
        public bool CorpseBuff = true;
        public bool InfiniteBuffs = true;


        public bool VariousTests = false;//does nothing

        public static Settings i = new Settings();
        public static string Path = System.IO.Path.Combine("QMods", "BuildAnywhere", "settings.json");

        public void ParseEnums()
        {   
            Enum.TryParse(ButtonIgnoreBuildArea, true, out _ButtonIgnoreBuildArea);
            Enum.TryParse(ButtonIgnoreBuildOverlap, true, out _ButtonIgnoreBuildOverlap);
            Enum.TryParse(TeleportStone, true, out _TeleportStone);
            Enum.TryParse(TeleportMouse, true, out _TeleportMouse);

            Debug.Log($"[BuildAnywhere] Mapping buttons: _ButtonIgnoreBuildArea={i._ButtonIgnoreBuildArea}, _ButtonIgnoreBuildOverlap={_ButtonIgnoreBuildOverlap}, _TeleportStone={_TeleportStone}, _TeleportMouse={_TeleportMouse}");
        }

        public bool Load()
        {
            try
            {
                if (!File.Exists(Path))
                    return false;

                JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    TypeNameHandling = TypeNameHandling.None,
                    MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                    PreserveReferencesHandling = PreserveReferencesHandling.None
                });

                using (StreamReader streamReader = new StreamReader(Path))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                    {
                        i = serializer.Deserialize<Settings>(jsonReader);

                        jsonReader.Close();
                    }

                    streamReader.Close();
                }
            }
            catch (Exception e)
            {
                Main.Log(e.ToString());
                return false;
            }
            
            return true;
        }
        public bool Save()
        {
            try
            {
                JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    TypeNameHandling = TypeNameHandling.None,
                    MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                    PreserveReferencesHandling = PreserveReferencesHandling.None
                });

                using (StreamWriter streamWriter = new StreamWriter(Path))
                {
                    using (JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter))
                    {
                        serializer.Serialize(jsonWriter, i);

                        jsonWriter.Close();
                    }

                    streamWriter.Close();
                }
            }
            catch (Exception e)
            {
                Main.Log(e.ToString());
                return false;
            }

            return true;
        }
    }
}