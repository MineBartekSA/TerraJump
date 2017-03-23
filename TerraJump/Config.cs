using System.IO;
using TShockAPI;
using Newtonsoft.Json;

namespace TerraJump
{
    public class Config
    {
        public bool ToggleJumpPads { get; set; }
        public int Height { get; set; }
        public int JBID { get; set; }
        public bool PressureTriggerEnable { get; set; }
        public string ReFormat { get; set; }
        public byte ReRed { get; set; }
        public byte ReGrean { get; set; }
        public byte ReBlue { get; set; }

        public static Config LoadProcedure(string path)
        {
            bool isit = File.Exists(path);
            if (isit)
            {
                TShock.Log.Info("Config file found!");
                var JSON = Load(path);
                return (JSON);
            }
            else
            {
                TShock.Log.Error("Config file not found!");
                var JSON = Create(path);
                return (JSON);
            }
        }
        public static Config Update(string path, bool TJP, int H, int JBID, bool PTE, string rForm, byte r, byte g, byte b)
        {
            TShock.Log.Info("Updating config");
            Config c = new Config
            {
                ToggleJumpPads = TJP,
                Height = H,
                JBID = JBID,
                PressureTriggerEnable = PTE,
                ReFormat = rForm,
                ReRed = r,
                ReGrean = g,
                ReBlue = b
            };
            File.WriteAllText(path, JsonConvert.SerializeObject(c, Formatting.Indented));

            StreamReader sr = new StreamReader(File.Open(path, FileMode.Open));
            var JSON = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            return (JSON);
        }

        public static Config Load(string path)
        {
            TShock.Log.Info("Loading config");
            try
            {
                StreamReader sr = new StreamReader(File.Open(path, FileMode.Open));
                var JSON = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
                return (JSON);
            }
            catch(JsonReaderException exe)
            {
                TShock.Log.Error("Error while Loading config");
                TShock.Log.Error(exe.Message);
                Config exeConf = new Config
                {
                    ToggleJumpPads = false,
                    Height = 20,
                    JBID = 193,
                    PressureTriggerEnable = true,
                    ReFormat = "<:group:> :user: : :mess:",
                    ReRed = 255,
                    ReGrean = 255,
                    ReBlue = 255
                };

                return (exeConf);
            }
            
        }

        public static Config Create(string path)
        {
            TShock.Log.Info("Creating a new config file");
            Config cd = new Config
            {
                ToggleJumpPads = true,
                Height = 20,
                JBID = 193,
                PressureTriggerEnable = true,
                ReFormat = "<:group:> :user: : :mess:",
                ReRed = 255,
                ReGrean = 255,
                ReBlue = 255
            };

            File.WriteAllText(path, JsonConvert.SerializeObject(cd, Formatting.Indented));

            StreamReader sr = new StreamReader(File.Open(path, FileMode.Open));
            var JSON = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            return (JSON);
        }
    }
}