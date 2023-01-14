using Newtonsoft.Json;
using TShockAPI;

namespace TerraJump;

public class Config
{
    [JsonIgnore]
    private static readonly string ConfigPath = Path.Combine(TShock.SavePath, "TerraJump.json");
    [JsonProperty("Enabled")]
    public bool Enabled { get; set; }
    [JsonProperty("JumpHeight")]
    public float Height { get; set; }
    [JsonProperty("TileID")]
    public int BlockId { get; set; }

    public Config()
    {
        Enabled = true;
        Height = 20;
        BlockId = 193;
    }

    public static void LoadProcedure(ref Config config)
    {
        if (File.Exists(ConfigPath))
            Load(out config, ConfigPath);
        else
            Create(config, ConfigPath);
    }

    private static void Load(out Config config, string path)
    {
        TShock.Log.Info("Loading config");
        try
        {
            using var sr = new StreamReader(File.Open(path, FileMode.Open));
            config = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd()) ?? throw new Exception("Failed to read configuration file");
        }
        catch(JsonReaderException exe)
        {
            TShock.Log.Error("Error while loading config");
            TShock.Log.Error(exe.Message);
            config = new Config();
        }
    }

    private static void Create(Config config, string path)
    {
        TShock.Log.Info("Creating a new config file");
        File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
    }

    public void Update()
    {
        TShock.Log.Info("Updating config");
        File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    public bool Reload()
    {
        TShock.Log.Info("Reloading config");
        try
        {
            Load(out var newConfig, ConfigPath);
            Enabled = newConfig.Enabled;
            BlockId = newConfig.BlockId;
            Height = newConfig.Height;
        }
        catch (JsonReaderException exe)
        {
            TShock.Log.Error("Error while reloading config");
            TShock.Log.Error(exe.Message);
            return false;
        }
        return true;
    }
}
