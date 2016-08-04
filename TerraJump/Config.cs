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
        public string JBID { get; set; }
        public bool pressureTriggerEnable { get; set; }
        public bool projectileTriggerEnable { get; set; }

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
        public static Config update(string path, bool TJP, int H, string JBID, bool PTE, bool PrTE)
        {
            Config c = new Config
            {
                toggleJumpPads = TJP,
                height = H,
                JBID = JBID,
                pressureTriggerEnable = PTE,
                projectileTriggerEnable = PrTE
            };
            File.WriteAllText(path, JsonConvert.SerializeObject(c, Formatting.Indented));

            StreamReader sr = new StreamReader(File.Open(path, FileMode.Open));
            var JSON = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            return (JSON);
        }

        public static Config load(string path)
        {
            StreamReader sr = new StreamReader(File.Open(path, FileMode.Open));
            var JSON = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            return (JSON);
        }

        public static Config create(string path)
        {
            Config cd = new Config
            {
                toggleJumpPads = true,
                height = 20,
                JBID = "SlimeBlcok",
                pressureTriggerEnable = true,
                projectileTriggerEnable = false
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
