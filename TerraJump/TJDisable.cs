using System.IO;
using TShockAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;

namespace TerraJump
{
    public class TJUDis
    {
        private static string _configFilePath = Path.Combine(TShock.SavePath, "DisPlayers.json");
        private static TJUDis c;

        public List<string> UList { get; set; }
        public DataSet XYSet { get; set; }

        public static TJUDis start()
        {
            bool exist = File.Exists(_configFilePath);
            TShock.Log.Info("Starting loading procedure!");
            if (exist)
            {
                c = load(_configFilePath);
            }
            else if (!exist)
            {
                c = create(_configFilePath);
            }
            return (c);
        }

        static TJUDis load(string path)
        {
            TShock.Log.Info("Tyring to load list of players");
            try
            {
                StreamReader sr = new StreamReader(File.Open(path, FileMode.Open));
                var UL = JsonConvert.DeserializeObject<TJUDis>(sr.ReadToEnd());
                TShock.Log.Info("Succes!");
                return (UL);
            }
            catch(JsonReaderException exe)
            {
                TShock.Log.Error("Error while Loading config");
                TShock.Log.Error(exe.Message);
                List<string> empty = new List<string>();
                TJUDis tj = new TJUDis { UList = empty, XYSet = new DataSet("XYJumpPads") };
                DataTable tXY = new DataTable("XYJumpPads");
                DataColumn ID = new DataColumn("ID", typeof(int));
                DataColumn X = new DataColumn("X", typeof(float));
                DataColumn Y = new DataColumn("Y", typeof(float));
                tXY.Columns.Add(ID);
                tXY.Columns.Add(X);
                tXY.Columns.Add(Y);
                tj.XYSet.Tables.Add(tXY);

                DataRow rXY = tXY.NewRow();
                rXY["ID"] = 0;
                rXY["X"] = 0;
                rXY["Y"] = 0;
                tXY.Rows.Add(rXY);

                tj.XYSet.AcceptChanges();

                return (tj);
            }
        }

        static TJUDis create(string path)
        {
            TShock.Log.Info("Createing player list");

            List<string> Ulisttttt = new List<string>
            {
                "TestUList"
            };

            TJUDis tjc = new TJUDis { UList = Ulisttttt, XYSet = new DataSet("XYJumpPads") };

            DataTable tXY = new DataTable("XYJumpPads");
            DataColumn ID = new DataColumn("ID", typeof(int));
            DataColumn X = new DataColumn("X", typeof(float));
            DataColumn Y = new DataColumn("Y", typeof(float));
            tXY.Columns.Add(ID);
            tXY.Columns.Add(X);
            tXY.Columns.Add(Y);
            tjc.XYSet.Tables.Add(tXY);

            DataRow rXY = tXY.NewRow();
            rXY["ID"] = 0;
            rXY["X"] = 0;
            rXY["Y"] = 0;
            tXY.Rows.Add(rXY);

            tjc.XYSet.AcceptChanges();

            File.WriteAllText(path, JsonConvert.SerializeObject(tjc, Formatting.Indented));

            return (tjc);
        }

        public static TJUDis add (TSPlayer user)
        {
            string name = user.Name;

            StreamReader sr = new StreamReader(File.Open(_configFilePath, FileMode.Open));
            var readed = JsonConvert.DeserializeObject<TJUDis>(sr.ReadToEnd());
            sr.Close();

            var lastetList = readed.UList;

            lastetList.Add(user.Name);

            TJUDis tjj = new TJUDis { UList = lastetList, XYSet = readed.XYSet };

            File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(tjj, Formatting.Indented));

            return (tjj);
        }

        public static TJUDis add(float x, float Y)
        {

            StreamReader sr = new StreamReader(File.Open(_configFilePath, FileMode.Open));
            var readed = JsonConvert.DeserializeObject<TJUDis>(sr.ReadToEnd());
            sr.Close();
            TShock.Log.Info("reading form reader");
            var lastetList = readed.UList;

            var XY = readed.XYSet;
            int lastetID = 0;
            XY.Tables["XYJumpPads"].AsEnumerable().ForEach(xyy =>
            {
                if (lastetID.Equals((int)xyy["ID"]))
                {
                    lastetID = (int)xyy["ID"];
                }
            });

            DataRow dr = XY.Tables["XYJumpPads"].NewRow();
            dr["ID"] = lastetID + 1;
            dr["X"] = x;
            dr["Y"] = Y;
            XY.Tables["XYJumpPads"].Rows.Add(dr);

            XY.AcceptChanges();
            TShock.Log.Info("Added now saving");
            TJUDis tjj = new TJUDis { UList = lastetList, XYSet = XY };

            File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(tjj, Formatting.Indented));
            TShock.Log.Info("Saveing to file complete");
            return (tjj);
        }

        public static TJUDis remove (TSPlayer user)
        {
            string name = user.Name;

            StreamReader sr = new StreamReader(File.Open(_configFilePath, FileMode.Open));
            var readed = JsonConvert.DeserializeObject<TJUDis>(sr.ReadToEnd());
            sr.Close();

            var lastetList = readed.UList;

            lastetList.Remove(name);

            TJUDis tjj = new TJUDis { UList = lastetList, XYSet = readed.XYSet };

            File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(tjj, Formatting.Indented));

            return (tjj);
        }

        public static TJUDis remove(float x, float y)
        {

            StreamReader sr = new StreamReader(File.Open(_configFilePath, FileMode.Open));
            var readed = JsonConvert.DeserializeObject<TJUDis>(sr.ReadToEnd());
            sr.Close();

            var lastetList = readed.UList;
            var a = new List<float> { x, y };
            var XY = readed.XYSet;

            XY.Tables["XYJumpPads"].AsEnumerable().ForEach(xyy =>
            {
                if ((x.Equals((float)xyy["X"])) && (y.Equals((float)xyy["Y"])))
                {
                    XY.Tables["XYJumpPads"].Rows.Remove(xyy);
                }
            });

            XY.AcceptChanges();

            TJUDis tjj = new TJUDis { UList = lastetList, XYSet = XY };

            File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(tjj, Formatting.Indented));

            return (tjj);
        }

        public static TJUDis read ()
        {
            StreamReader sr = new StreamReader(File.Open(_configFilePath, FileMode.Open));
            var readed = JsonConvert.DeserializeObject<TJUDis>(sr.ReadToEnd());

            return (readed);
        }
    }
}
