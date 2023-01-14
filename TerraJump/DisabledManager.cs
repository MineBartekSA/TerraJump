using Newtonsoft.Json;
using TShockAPI.DB;
using System.Data;
using TShockAPI;
using Terraria;

namespace TerraJump;

public class DisabledManager
{
    public List<TjUser> UserList { get; }
    public List<PadPoint> DisabledPads { get; }
    public List<PadDisable> PadDisables { get; }

    public DisabledManager()
    {
        UserList = new List<TjUser>();
        DisabledPads = new List<PadPoint>();
        PadDisables = new List<PadDisable>();
    }

    public void Start()
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

        if (tju)
        {
            var query = TShock.DB.QueryReader("SELECT * FROM TJUsers");
            while (query.Read())
            {
                var user = new TjUser
                {
                    Uuid = query.Get<string>("uuid"),
                    SelfDisabled = query.Get<int>("disabled") == 1,
                    DisabledPads = JsonConvert.DeserializeObject<List<PadPoint>>(query.Get<string>("pads")) ?? new List<PadPoint>()
                };
                user.DisabledPads ??= new List<PadPoint>();
                UserList.Add(user);
            }
        }

        if (tjp)
        {
            var query = TShock.DB.QueryReader("SELECT * FROM TJPads");
            while (query.Read())
            {
                DisabledPads.Add(new PadPoint()
                {
                    X = query.Get<int>("x"),
                    Y = query.Get<int>("y")
                });
            }
        }

        TryConvert();
    }

    public bool AddUser(TjUser user)
    {
        if (TShock.DB.Query("INSERT INTO TJUsers(uuid, disabled, pads) VALUES (@0, @1, @2)", user.Uuid, user.SelfDisabled.ToInt(), JsonConvert.SerializeObject(user.DisabledPads)) != 1)
        {
            TShock.Log.Error("SQL Error while adding user!");
            return false;
        }
        UserList.Add(user);
        return true;
    }

    public bool ModifyUser(TjUser user)
    {
        var res = TShock.DB.Query("UPDATE TJUsers SET disabled=@0, pads=@1 WHERE uuid=@2", user.SelfDisabled.ToInt(), JsonConvert.SerializeObject(user.DisabledPads), user.Uuid);
        if (res != 1)
        {
            TShock.Log.Error($"SQL Error while updating user! ({res})");
            return false;
        }
        UserList.RemoveAll(u => u.Uuid == user.Uuid);
        UserList.Add(user);
        return true;
    }

    public bool AddPad(PadPoint pad)
    {
        if (TShock.DB.Query("INSERT INTO TJPads(x, y) VALUES (@0, @1)", pad.X, pad.Y) != 1)
        {
            TShock.Log.Error("SQL Error while adding pad point!");
            return false;
        }
        DisabledPads.Add(pad);
        return true;
    }

    public bool RemovePad(PadPoint pad)
    {
        if (TShock.DB.Query("DELETE FROM TJPads WHERE x=@0 AND y=@1", pad.X, pad.Y) != 1)
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
            var json = JsonConvert.DeserializeObject<OldJson>(sr.ReadToEnd()) ?? throw new Exception("Failed to read json file");
            sr.Close();
            fi.Close();
            var dataRowCollection = json.PadPointSet.Tables["XYJumpPads"]?.Rows;
            if (dataRowCollection != null)
                foreach (DataRow row in dataRowCollection)
                    DisabledPads.Add(new PadPoint { X = Convert.ToInt32(row["X"]), Y = Convert.ToInt32(row["Y"]) });
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

        private OldJson()
        {
            UserList = new List<string>();
            PadPointSet = new DataSet();
        }
    }
}
