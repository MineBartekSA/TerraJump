using TShockAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Data;
using System.IO;
using Terraria;
using TShockAPI.DB;

namespace TerraJump
{
    public class DisabledManager
    {
        public List<TjUser> UserList { get; private set; }
        public List<PadPoint> DisabledPads { get; private set; }
        public List<PadDisable> PadDisables { get; private set; }

        public static DisabledManager Start()
        {
            bool tju = false, tjp = false;
            try
            {
                TShock.DB.Query("CREATE TABLE TJUsers(uuid TEXT, disabled INT2, pads TEXT)");
            }
            catch (Exception exe)
            {
                if (exe.HResult != -2147467259)
                    throw new Exception("SQL ERROR: " + exe.HResult);
                tju = true;
            }

            try
            {
                TShock.DB.Query("CREATE TABLE TJPads(x INT, y INT)");
            }
            catch (Exception exe)
            {
                if (exe.HResult != -2147467259)
                    throw new Exception("SQL ERROR: " + exe.HResult);
                tjp = true;
            }

            var dm = new DisabledManager
            {
                UserList = new List<TjUser>(),
                DisabledPads = new List<PadPoint>(),
                PadDisables = new List<PadDisable>()
            };

            if (tju)
            {
                var query = TShock.DB.QueryReader("SELECT * FROM TJUsers");
                while (query.Read())
                {
                    var user = new TjUser()
                    {
                        Uuid = query.Get<string>("uuid"),
                        SelfDisabled = query.Get<int>("disabled") == 1,
                        DisabledPads = JsonConvert.DeserializeObject<List<PadPoint>>(query.Get<string>("pads"))
                    };
                    if (user.DisabledPads == null)
                        user.DisabledPads = new List<PadPoint>();
                    dm.UserList.Add(user);
                }
            }

            if (tjp)
            {
                var query = TShock.DB.QueryReader("SELECT * FROM TJPads");
                while (query.Read())
                {
                    dm.DisabledPads.Add(new PadPoint()
                    {
                        X = query.Get<int>("x"),
                        Y = query.Get<int>("y")
                    });
                }
            }

            dm.TryConvert();

            return dm;
        }

        public bool AddUser(TjUser user)
        {
            if (TShock.DB.Query($"INSERT INTO TJUsers(uuid, disabled, pads) VALUES ('{user.Uuid}', {user.SelfDisabled.ToInt()}, '{JsonConvert.SerializeObject(user.DisabledPads)}')") != 1)
            {
                TShock.Log.Error("SQL Error while adding user!");
                return false;
            }
            UserList.Add(user);
            return true;
        }

        public bool ModifyUser(TjUser user)
        {
            if (TShock.DB.Query($"UPDATE TJUsers SET disabled={user.SelfDisabled.ToInt()}, pads='{JsonConvert.SerializeObject(user.DisabledPads)}' WHERE uuid='{user.Uuid}'") != 1)
            {
                TShock.Log.Error("SQL Error while updating user!");
                return false;
            }
            UserList.RemoveAll(u => u.Uuid == user.Uuid);
            UserList.Add(user);
            return true;
        }

        public bool AddPad(PadPoint pad)
        {
            if (TShock.DB.Query($"INSERT INTO TJPads(x, y) VALUES ({pad.X}, {pad.Y})") != 1)
            {
                TShock.Log.Error("SQL Error while adding pad point!");
                return false;
            }
            DisabledPads.Add(pad);
            return true;
        }

        public bool RemovePad(PadPoint pad)
        {
            if (TShock.DB.Query($"DELETE FROM TJPads WHERE x={pad.X} AND y={pad.Y}") != 1)
            {
                TShock.Log.Error("SQL Error while removing pad point!");
                return false;
            }
            DisabledPads.RemoveAll(p => p.X == pad.X && p.Y == pad.Y);
            return true;
        }

        public struct TjUser
        {
            public string Uuid;
            public bool SelfDisabled;
            public List<PadPoint> DisabledPads;
        }

        public struct PadPoint
        {
            public int X;
            public int Y;
        }

        public struct PadDisable
        {
            public string Uuid;
            public bool IsSelf;
            public DateTime Started;
        }

        private void TryConvert()
        {
            var path = Path.Combine(TShock.SavePath, "DisPlayers.json");
            if (!File.Exists(path)) return;

            TShock.Log.Info("Converting old 'DisPlayers.json' file");
            try
            {
                var fi = File.Open(path, FileMode.Open);
                var sr = new StreamReader(fi);
                var json = JsonConvert.DeserializeObject<OldJson>(sr.ReadToEnd());
                sr.Close();
                fi.Close();
                foreach (DataRow row in json.PadPointSet.Tables["XYJumpPads"].Rows)
                    DisabledPads.Add(new PadPoint {X = Convert.ToInt32(row["X"]), Y = Convert.ToInt32(row["Y"])});
                foreach (var user in json.UserList)
                {
                    var player = TShock.UserAccounts.GetUserAccountByName(user);
                    if (player != null)
                        UserList.Add(new TjUser
                        {
                            Uuid = player.UUID,
                            SelfDisabled = false,
                            DisabledPads = new List<PadPoint>()
                        });
                    else
                        TShock.Log.Error($"Player not found with username '{user}'!");
                }
            }
            catch
            {
                TShock.Log.Info("The 'DisPlayers.json' file is invalid! Removing...");
            }

            File.Delete(path);
            TShock.Log.Info("Conversion complete!");
        }

        private class OldJson
        {
            [JsonProperty("UList")]
            public List<string> UserList { get; set; }
            [JsonProperty("XYSet")]
            public DataSet PadPointSet { get; set; }
        }
    }
}
