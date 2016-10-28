using System.IO;
using TShockAPI;
using Newtonsoft.Json;
using System.Data;

namespace TerraJump
{
    public class Config
    {
        public bool toggleJumpPads { get; set; }
        public int height { get; set; }
        public int JBID { get; set; }
        public bool pressureTriggerEnable { get; set; }
        public string reFormat { get; set; }
        public byte ReRed { get; set; }
        public byte ReGrean { get; set; }
        public byte ReBlue { get; set; }

        public static Config loadProcedure(string path)
        {
            bool isit = File.Exists(path);
            if (isit)
            {
                TShock.Log.Info("Config file found!");
                var JSON = load(path);
                return (JSON);
            }
            else
            {
                TShock.Log.Error("Config file not found!");
                var JSON = create(path);
                return (JSON);
            }
        }
        public static Config update(string path, bool TJP, int H, int JBID, bool PTE, string rForm, byte r, byte g, byte b)
        {
            TShock.Log.Info("Updating config");
            Config c = new Config
            {
                toggleJumpPads = TJP,
                height = H,
                JBID = JBID,
                pressureTriggerEnable = PTE,
                reFormat = rForm,
                ReRed = r,
                ReGrean = g,
                ReBlue = b
            };
            File.WriteAllText(path, JsonConvert.SerializeObject(c, Formatting.Indented));

            StreamReader sr = new StreamReader(File.Open(path, FileMode.Open));
            var JSON = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            return (JSON);
        }

        public static Config load(string path)
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
                    toggleJumpPads = false,
                    height = 20,
                    JBID = 193,
                    pressureTriggerEnable = true,
                    reFormat = "<:group:> :user: : :mess:",
                    ReRed = 255,
                    ReGrean = 255,
                    ReBlue = 255
                };

                return (exeConf);
            }
            
        }

        public static Config create(string path)
        {
            TShock.Log.Info("Creating a new config file");
            Config cd = new Config
            {
                toggleJumpPads = true,
                height = 20,
                JBID = 193,
                pressureTriggerEnable = true,
                reFormat = "<:group:> :user: : :mess:",
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
    
    /*public static class configDetiles
    {
        public bool enable { get; set; }
        public DataSet achSet { get; set; }
    }*/
}