using System.IO;
using TShockAPI;
using Newtonsoft.Json;

namespace TerraJump
{
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

        public static Config LoadProcedure()
        {
            if (File.Exists(ConfigPath))
                return Load(ConfigPath);
            return Create(ConfigPath);
        }

        private static Config Load(string path)
        {
            TShock.Log.Info("Loading config");
            try
            {
                var sr = new StreamReader(File.Open(path, FileMode.Open));
                return JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            }
            catch(JsonReaderException exe)
            {
                TShock.Log.Error("Error while Loading Config");
                TShock.Log.Error(exe.Message);
                var exeConf = new Config
                {
                    Enabled = false,
                    Height = 20,
                    BlockId = 193
                };
                return exeConf;
            }
        }

        private static Config Create(string path)
        {
            TShock.Log.Info("Creating a new Config file");
            var cd = new Config
            {
                Enabled = true,
                Height = 20,
                BlockId = 193
            };
            File.WriteAllText(path, JsonConvert.SerializeObject(cd, Formatting.Indented));

            var sr = new StreamReader(File.Open(path, FileMode.Open));
            return JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
        }

        public void Update()
        {
            TShock.Log.Info("Updating Config");
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public bool Reload()
        {
            TShock.Log.Info("Reloading Config");
            try
            {
                var sr = new StreamReader(File.Open(ConfigPath, FileMode.Open));
                var json = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
                Enabled = json.Enabled;
                BlockId = json.BlockId;
                Height = json.Height;
            }
            catch (JsonReaderException exe)
            {
                TShock.Log.Error("Error while Reloading Config");
                TShock.Log.Error(exe.Message);
                return false;
            }
            return true;
        }
    }
}