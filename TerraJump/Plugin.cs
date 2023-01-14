using Microsoft.Xna.Framework;
using TerrariaApi.Server;
using Terraria.Enums;
using TShockAPI;
using Terraria;

namespace TerraJump;

[ApiVersion(2, 1)]
public class TerraJump : TerrariaPlugin
{
    private static readonly DisabledManager DMgr = new ();
    private static Config _config = new ();
    private Version? _updates;
    private int _tick = 1;

    public override string Name => "TerraJump";
    public override Version Version => typeof(TerraJump).Assembly.GetName().Version ?? throw new Exception("Failed to fetch assembly version");
    public override string Author => "MineBartekSA";
    public override string Description => "A simple Jump Pads plugin for TShock!";

    public TerraJump(Main game) : base(game) { }

    public override void Initialize()
    {
        CheckUpdates();
        Config.LoadProcedure(ref _config);
        DMgr.Start();
        if(!TShock.ServerSideCharacterConfig.Settings.Enabled)
        {
            TShock.Log.ConsoleError("[TerraJump] You need to enable SSC!");
            return;
        }
        ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
        // ServerApi.Hooks.PlayerTriggerPressurePlate.Register(this, OnPlayerTriggerPressurePlate); // Broken as of 14.01.2023 (OTAPI.Hooks.Collision.PressurePlate)
        On.Terraria.Wiring.HitSwitch += OnHitSwitch;
        ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
        TShock.Log.Info("Initialization complete!");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
            // ServerApi.Hooks.PlayerTriggerPressurePlate.Deregister(this, OnPlayerTriggerPressurePlate); // Broken as of 14.01.2023 (OTAPI.Hooks.Collision.PressurePlate)
            On.Terraria.Wiring.HitSwitch -= OnHitSwitch;
            ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
        }
        base.Dispose(disposing);
    }

    private void OnInitialize(EventArgs args)
    {
        Commands.ChatCommands.Add(new Command(TerraJumpCommand, "terrajump", "tj")
        {
            HelpText = "TerraJump main command"
        });
        Commands.ChatCommands.Add(new Command("terrajump.use", Jump, "jump", "j")
        {
            HelpText = "Jump command"
        });
    }

    private void OnUpdate(EventArgs args)
    {
        if (_tick == 60)
        {
            _tick = 1;
            var l = new List<string>();
            foreach (var dis in DMgr.PadDisables.Where(dis => dis.Started.Add(TimeSpan.FromSeconds(20)) <= DateTime.Now))
            {
                TShock.Players.First(p => p.UUID == dis.Uuid).SendInfoMessage("You are no longer going to disable the next jump pad");
                l.Add(dis.Uuid);
            }
            l.ForEach(uuid => DMgr.PadDisables.RemoveAll(user => user.Uuid == uuid));
        }
        _tick++;
    }

    #region Command Functions
    private void TerraJumpCommand(CommandArgs args)
    {
        if (!_config.Enabled)
        {
            if (args.Parameters.Count >= 1 && args.Parameters[0] == "toggle")
                Toggle(args);
            else
                args.Player.SendErrorMessage("TerraJump is currently disabled! Use /terrajump toggle to enabled it");
            return;
        }
        if (args.Parameters.Count == 0)
            args.Parameters.Add("null");
        switch (args.Parameters[0])
        {
            case "t":
            case "tog":
            case "toggle":
                Toggle(args);
                break;
            case "e":
            case "ed":
            case "edit":
                Edit(args);
                break;
            case "d":
            case "dis":
            case "disable":
                Disable(args);
                break;
            case "r":
            case "rel":
            case "reload":
                Reload(args);
                break;
            default:
                SendHelp(args.Player);
                break;
        }
    }

    private void SendHelp(TSPlayer player)
    {
        player.SendSuccessMessage("TerraJump v{0}", Version);
        if (_updates != null)
            player.SendMessage($"Update to version {_updates} is available!", Color.Aqua);
        player.SendInfoMessage("Usage: /terrajump <command>");
        player.SendInfoMessage("Commands:");
        player.SendInfoMessage(" - toggle - Toggle TerraJump");
        player.SendInfoMessage(" - edit - Edit jump pad block and force");
        player.SendInfoMessage(" - disable - Disable jump pads");
        player.SendInfoMessage(" - reload - Reload config");
        player.SendInfoMessage("You can also use the /jump command to launch yourself");
    }

    private void Toggle(CommandArgs args)
    {
        if (!args.Player.HasPermission("terrajump.admin.toggle"))
        {
            args.Player.SendErrorMessage("You don't have enough permissions to use this command!");
            return;
        }
        _config.Enabled = !_config.Enabled;
        _config.Update();
        TShock.Log.ConsoleInfo(args.Player.Name + " toggled TerraJump");
        args.Player.SendSuccessMessage("TerraJump is now {0}!", (_config.Enabled) ? "Enabled" : "Disabled");
    }

    private void Edit(CommandArgs args)
    {
        if (!args.Player.HasPermission("terrajump.admin.edit"))
        {
            args.Player.SendErrorMessage("You don't have enough permissions to use this command!");
            return;
        }
        if (args.Parameters.Count == 1)
            args.Parameters.Add("null");
        switch (args.Parameters[1])
        {
            case "t":
            case "tile":
                ChangeTile(args);
                break;
            case "f":
            case "force":
                ChangeHeight(args);
                break;
            default:
                args.Player.SendInfoMessage("Usage: /terrajump edit <subcommand> [param]");
                args.Player.SendInfoMessage("Subcommands:");
                args.Player.SendInfoMessage(" - tile [tile id] - Check or change the jump pad tile");
                args.Player.SendInfoMessage(" - force [number] - Check or change the jump height");
                break;
        }
    }

    private void ChangeTile(CommandArgs args)
    {
        if (args.Parameters.Count <= 2)
        {
            var tile = (TileIDEnum) _config.BlockId;
            args.Player.SendInfoMessage("Current jump pads tile is {0} ({1})", tile, _config.BlockId);
            return;
        }
        var id = (int)float.Parse(args.Parameters[2]);
        if (!Enum.TryParse($"{id}", out TileIDEnum tileName))
        {
            args.Player.SendErrorMessage("Invalid tile id!");
            return;
        }
        _config.BlockId = id;
        args.Player.SendSuccessMessage("You've set the jump pad tile to {0} ({1})", tileName, id);
        TShock.Log.Info("Jump Pad Tile ID = {0}", _config.BlockId);
        _config.Update();
    }

    private void ChangeHeight(CommandArgs args)
    {
        if (args.Parameters.Count <= 2)
        {
            args.Player.SendInfoMessage("Currently the force is set to {0}", _config.Height);
            return;
        }
        var nh = float.Parse(args.Parameters[2]);
        if(nh < 10)
        {
            args.Player.SendErrorMessage("You can't set jump force to less than 10!");
            return;
        }
        _config.Height = nh;
        args.Player.SendSuccessMessage("You've set the jump force to {0}", _config.Height);
        TShock.Log.Info("Jump Height = {0}", _config.Height);
        _config.Update();
    }

    private void Reload(CommandArgs args)
    {
        if (!args.Player.HasPermission("terrajump.admin.reload"))
        {
            args.Player.SendErrorMessage("You don't have enough permission to use this command!");
            return;
        }
        if (_config.Reload())
            args.Player.SendSuccessMessage("Reload complete!");
        else
            args.Player.SendErrorMessage("Failed to reload config!");
    }

    private void Disable(CommandArgs args)
    {
        var admin = args.Player.HasPermission("terrajump.admin.disable");
        if (!(admin || args.Player.HasPermission("terrajump.disable")))
        {
            args.Player.SendErrorMessage("You don't have enough permissions to use this command!");
            return;
        }
        if (args.Parameters.Count == 1)
            args.Parameters.Add("null");
        switch (args.Parameters[1])
        {
            case "s":
            case "self":
                DisableSelf(args);
                break;
            case "p":
            case "pad":
                DisablePad(args, true);
                break;
            case "g":
            case "global":
                if (admin)
                    DisablePad(args, false);
                else
                    args.Player.SendErrorMessage("You don't have enough permission to use this command!");
                break;
            default:
                args.Player.SendInfoMessage("Usage: /terrajump disable <subcommand>");
                args.Player.SendInfoMessage("Subcommands:");
                args.Player.SendInfoMessage(" - self - Disable all jump pads for yourself");
                args.Player.SendInfoMessage(" - pad - Disable a specific jump pad for yourself");
                if (admin)
                    args.Player.SendInfoMessage(" - global - Disable a specific jump pad globally");
                break;
        }
    }

    private void DisableSelf(CommandArgs args)
    {
        if (DMgr.UserList.Count(u => u.Uuid == args.Player.UUID) == 0)
        {
            if (DMgr.AddUser(new DisabledManager.TjUser
                {
                    Uuid = args.Player.UUID,
                    SelfDisabled = true
                }))
                args.Player.SendSuccessMessage("You've disabled all jump pads for yourself!");
            else
                args.Player.SendErrorMessage("Failed to disable jump pads!");
            return;
        }
        var user = DMgr.UserList.First(u => u.Uuid == args.Player.UUID);
        if (user.SelfDisabled)
        {
            user.SelfDisabled = false;
            if (DMgr.ModifyUser(user))
                args.Player.SendSuccessMessage("You've enabled all jump pads for yourself!");
            else
                args.Player.SendErrorMessage("Failed to enable jump pads!");
            return;
        }

        user.SelfDisabled = true;
        if (DMgr.ModifyUser(user))
            args.Player.SendSuccessMessage("You've disabled all jump pads for yourself!");
        else
            args.Player.SendErrorMessage("Failed to disable jump pads!");
    }

    private void DisablePad(CommandArgs args, bool isSelf)
    {
        DMgr.PadDisables.Add(new DisabledManager.PadDisable
        {
            Uuid = args.Player.UUID,
            IsSelf = isSelf,
            Started = DateTime.Now
        });
        args.Player.SendInfoMessage("Please stand on a jump pad you want do disable");
    }

    private void Jump(CommandArgs args)
    {
        if (!_config.Enabled)
            return;
        if (DMgr.UserList.Any(u => u.Uuid == args.Player.UUID && u.SelfDisabled))
            return;
        Thread.Sleep(250);
        args.Player.TPlayer.velocity.Y -= _config.Height;
        TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", args.Player.Index);
        args.Player.SendInfoMessage("Jump!");
    }
    #endregion

    private void OnHitSwitch(On.Terraria.Wiring.orig_HitSwitch orig, int x, int y)
    {
        var tile = Main.tile[x, y];
        if (Wiring.CurrentUser != 255 && tile.type is 135 or 428)
            HandlePlayerTriggerPressurePlate(TShock.Players[Wiring.CurrentUser], x, y);
        orig(x, y);
    }

    private static void HandlePlayerTriggerPressurePlate(TSPlayer owner, int x, int y)
    {
        if (!_config.Enabled)
            return;
        var underBlock = Main.tile[x, y + 1];
        if (underBlock.type != _config.BlockId)
            return;
        var underLeftBlock = Main.tile[x - 1, y + 1];
        var underRightBlock = Main.tile[x + 1, y + 1];
        if (underRightBlock.type != _config.BlockId || underLeftBlock.type != _config.BlockId)
            return;
        if (!owner.HasPermission("terrajump.use"))
        {
            owner.SendErrorMessage("You don't have enough permissions to do this!");
            return;
        }
        try
        {
            if (DMgr.PadDisables.Any(u => u.Uuid == owner.UUID))
            {
                var dis = DMgr.PadDisables.Find(u => u.Uuid == owner.UUID);
                if (dis.IsSelf)
                {
                    DMgr.PadDisables.Remove(dis);
                    if (DMgr.UserList.All(u => u.Uuid != owner.UUID))
                    {
                        if (DMgr.AddUser(new DisabledManager.TjUser
                            {
                                Uuid = owner.UUID,
                                SelfDisabled = false,
                                DisabledPads = new List<DisabledManager.PadPoint> { new () { X = x, Y = y } }
                            }))
                        {
                            owner.SendSuccessMessage("Successfully disabled jump pad!");
                            return;
                        }

                        owner.SendErrorMessage("Failed to disable this jump pad!");
                    }

                    var user = DMgr.UserList.Find(u => u.Uuid == owner.UUID);
                    if (user.DisabledPads.RemoveAll(p => p.X == x && p.Y == y) == 0)
                    {
                        user.DisabledPads.Add(new DisabledManager.PadPoint {X = x, Y = y});
                        if (DMgr.ModifyUser(user))
                        {
                            owner.SendSuccessMessage("Successfully disabled jump pad!");
                            return;
                        }

                        owner.SendErrorMessage("Failed to disable jump pad!");
                    }

                    if (DMgr.ModifyUser(user))
                        owner.SendSuccessMessage("Successfully enabled jump pad!");
                    else
                    {
                        owner.SendErrorMessage("Failed to enabled jump pad!");
                        return;
                    }
                }
                else
                {
                    DMgr.PadDisables.Remove(dis);
                    if (!DMgr.DisabledPads.Any(p => p.X == x && p.Y == y))
                    {
                        if (DMgr.AddPad(new DisabledManager.PadPoint {X = x, Y = y}))
                        {
                            owner.SendSuccessMessage("Successfully disabled jump pad globally! (X: {0} Y: {1})", x, y);
                            return;
                        }

                        owner.SendErrorMessage("Failed to disable jump pad globally!");
                    }
                    else
                    {
                        if (DMgr.RemovePad(DMgr.DisabledPads.Find(p => p.X == x && p.Y == y)))
                            owner.SendSuccessMessage("Successfully enabled jump pad globally! (X: {0} Y: {1})", x, y);
                        else
                        {
                            owner.SendErrorMessage("Failed to enable jump pad globally!");
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            TShock.Log.Error($"Exception!\n{e}\n{e.StackTrace}");
        }
        if (DMgr.UserList.Any(u => u.Uuid == owner.UUID && (u.SelfDisabled || u.DisabledPads.Any(p => p.X == x && p.Y == y))))
            return;
        if (DMgr.DisabledPads.Any(p => p.X == x && p.Y == y))
            return;
        owner.TPlayer.velocity.Y -= _config.Height;
        TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", owner.Index);
    }

    private async void CheckUpdates()
    {
        string latest;
        try
        {
            var http = new HttpClient();
            var response = await http.GetAsync("https://raw.githubusercontent.com/MineBartekSA/TerraJump/master/version.txt");
            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to fetch latest version number");
            latest = await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            TShock.Log.Error($"Unable to check for updates! {e.Message}");
            return;
        }

        var parts = new int[3];
        var lastPart = "";
        var partNum = 0;
        foreach (var t in latest)
        {
            if (t == '.')
            {
                if (int.TryParse(lastPart, out var part))
                {
                    parts[partNum] = part;
                    partNum++;
                    if (partNum == 3)
                        break;
                }
                lastPart = "";
                continue;
            }
            lastPart += t;
        }

        var latestVer = new Version(parts[0], parts[1], parts[2]);
        if (latestVer <= Version)
            return;

        TShock.Log.ConsoleInfo($"[TerraJump Update] New version available! New version {latestVer}");
        _updates = latestVer;
    }
}
