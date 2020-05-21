using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using System.Net;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace TerraJump
{
    [ApiVersion(2, 1)]
    public class TerraJump : TerrariaPlugin
    {
        private readonly Version ver = new Version(2, 3, 0);
        private static Config _config;
        private static DisabledManager _dMgr;
        private Version updates;
        private string getVer;
        private int tick = 1;

        public override string Name => "TerraJump";
        public override Version Version => ver;
        public override string Author => "MineBartekSA";
        public override string Description => "It's a simple Jump Pads plugin for TShock!";

        public TerraJump(Main game) : base(game) { }

        public override void Initialize()
        {
            CheckUpdates();
            _config = Config.LoadProcedure();
            _dMgr = DisabledManager.Start();
            if(!TShock.ServerSideCharacterConfig.Enabled)
            {
                TShock.Log.ConsoleError("[TerraJump]You need enable SSC!");
                return;
            }
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.PlayerTriggerPressurePlate.Register(this, OnPlayerTriggerPressurePlate);
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
            TShock.Log.Info("Initialization complete!");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.PlayerTriggerPressurePlate.Deregister(this, OnPlayerTriggerPressurePlate);
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
            if (tick == 60)
            {
                tick = 1;
                var l = new List<string>();
                foreach (var dis in _dMgr.PadDisables)
                {
                    if (dis.Started.Add(TimeSpan.FromSeconds(20)) > DateTime.Now) continue;
                    TShock.Players.First(p => p.UUID == dis.Uuid).SendInfoMessage("You are no longer going to disable next jump pad");
                    l.Add(dis.Uuid);
                }
                l.ForEach(uuid => _dMgr.PadDisables.RemoveAll(user => user.Uuid == uuid));
            }
            tick++;
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
            player.SendSuccessMessage("TerraJump v{0}", ver);
            if (updates != null)
                player.SendMessage($"Update to version {updates} is available!", Color.Aqua);
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
            args.Player.SendSuccessMessage("TerraJump now is {0}!", (_config.Enabled) ? "Enabled" : "Disabled");
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
                var tile = typeof(TileID).GetFields().Where(t => (ushort) t.GetValue(null) == _config.BlockId).Select(t => t.Name).First();
                args.Player.SendInfoMessage("Current jump pads tile is {0} ({1})", tile, _config.BlockId);
                return;
            }
            var id = (int)float.Parse(args.Parameters[2]);
            var tileName = typeof(TileID).GetFields().Where(t => (ushort)t.GetValue(null) == id).Select(t => t.Name).First();
            if (tileName == null)
            {
                args.Player.SendErrorMessage("Invalid tile id!");
                return;
            }
            _config.BlockId = id;
            args.Player.SendSuccessMessage("You have set jump pad tile to {0}", tileName);
            TShock.Log.ConsoleInfo("Jump Pad Tile ID = {0}", _config.BlockId);
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
            args.Player.SendSuccessMessage("You have set the jump force to {0}", _config.Height);
            TShock.Log.ConsoleInfo("Jump Height = {0}", _config.Height);
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
            if (!(args.Player.HasPermission("terrajump.admin.disable") || args.Player.HasPermission("terrajump.disable")))
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
                    if (args.Player.HasPermission("terrajump.admin.disable"))
                        DisablePad(args, false);
                    else
                        args.Player.SendErrorMessage("You don't have enough permission to use this command!");
                    break;
                default:
                    args.Player.SendInfoMessage("Usage: /terrajump disable <subcommand>");
                    args.Player.SendInfoMessage("Subcommands:");
                    args.Player.SendInfoMessage(" - self - Disable jump pads for self");
                    args.Player.SendInfoMessage(" - pad - Disable specific jump pad");
                    args.Player.SendInfoMessage(" - global - Disable a jump pad globally");
                    break;
            }
        }

        private void DisableSelf(CommandArgs args)
        {
            if (_dMgr.UserList.Count(u => u.Uuid == args.Player.UUID) == 0)
            {
                if (_dMgr.AddUser(new DisabledManager.TjUser
                {
                    Uuid = args.Player.UUID,
                    SelfDisabled = true
                }))
                    args.Player.SendSuccessMessage("You have disabled jump pads for yourself!");
                else
                    args.Player.SendErrorMessage("Failed to disable jump pads!");
                return;
            }
            var user = _dMgr.UserList.First(u => u.Uuid == args.Player.UUID);
            if (user.SelfDisabled)
            {
                user.SelfDisabled = false;
                if (_dMgr.ModifyUser(user))
                    args.Player.SendSuccessMessage("You have enabled jump pads for yourself!");
                else
                    args.Player.SendErrorMessage("Failed to enable jump pads!");
                return;
            }

            user.SelfDisabled = true;
            if (_dMgr.ModifyUser(user))
                args.Player.SendSuccessMessage("You have disabled jump pads for yourself!");
            else
                args.Player.SendErrorMessage("Failed to disable jump pads!");
        }

        private void DisablePad(CommandArgs args, bool isSelf)
        {
            _dMgr.PadDisables.Add(new DisabledManager.PadDisable
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
            if (_dMgr.UserList.Count(u => u.Uuid == args.Player.UUID && u.SelfDisabled) != 0)
                return;
            Thread.Sleep(500);
            args.Player.TPlayer.velocity.Y = args.Player.TPlayer.velocity.Y - _config.Height;
            TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", args.Player.Index);
            args.Player.SendInfoMessage("Jump!");
        }
        #endregion

        private void OnPlayerTriggerPressurePlate(TriggerPressurePlateEventArgs<Player> args)
        {
            if (!_config.Enabled)
                return;
            var underBlock = Main.tile[args.TileX, args.TileY + 1];
            if (underBlock.type != _config.BlockId)
                return;
            var underLeftBlock = Main.tile[args.TileX - 1, args.TileY + 1];
            var underRightBlock = Main.tile[args.TileX + 1, args.TileY + 1];
            if (underRightBlock.type != _config.BlockId || underLeftBlock.type != _config.BlockId)
                return;
            if (!TShock.Players[args.Object.whoAmI].HasPermission("terrajump.use"))
            {
                TShock.Players[args.Object.whoAmI].SendErrorMessage("You don't have enough permissions to do this!");
                return;
            }
            var ow = TShock.Players[args.Object.whoAmI];
            try
            {
                if (_dMgr.PadDisables.Count(u => u.Uuid == ow.UUID) != 0)
                {
                    var dis = _dMgr.PadDisables.Find(u => u.Uuid == ow.UUID);
                    if (dis.IsSelf)
                    {
                        _dMgr.PadDisables.Remove(dis);
                        if (_dMgr.UserList.Count(u => u.Uuid == ow.UUID) == 0)
                        {
                            if (_dMgr.AddUser(new DisabledManager.TjUser
                            {
                                Uuid = ow.UUID,
                                SelfDisabled = false,
                                DisabledPads = {new DisabledManager.PadPoint {X = args.TileX, Y = args.TileY}}
                            }))
                            {
                                ow.SendSuccessMessage("Successfully disabled this jump pad for you!");
                                return;
                            }

                            ow.SendErrorMessage("Failed to disable this jump pad for you!");
                        }

                        var user = _dMgr.UserList.Find(u => u.Uuid == ow.UUID);
                        if (user.DisabledPads.RemoveAll(p => p.X == args.TileX && p.Y == args.TileY) == 0)
                        {
                            user.DisabledPads.Add(new DisabledManager.PadPoint {X = args.TileX, Y = args.TileY});
                            if (_dMgr.ModifyUser(user))
                            {
                                ow.SendSuccessMessage("Successfully disabled this jump pad for you!");
                                return;
                            }

                            ow.SendErrorMessage("Failed to disable this jump pad for you!");
                        }

                        if (_dMgr.ModifyUser(user))
                            ow.SendSuccessMessage("Successfully enabled this jump pad for you!");
                        else
                        {
                            ow.SendErrorMessage("Failed to enabled this jump pad for you!");
                            return;
                        }
                    }
                    else
                    {
                        _dMgr.PadDisables.Remove(dis);
                        if (_dMgr.DisabledPads.Count(p => p.X == args.TileX && p.Y == args.TileY) == 0)
                        {
                            if (_dMgr.AddPad(new DisabledManager.PadPoint {X = args.TileX, Y = args.TileY}))
                            {
                                ow.SendSuccessMessage("Successfully disabled this jump pad! (X: {0} Y: {1})",
                                    args.TileX, args.TileY);
                                return;
                            }

                            ow.SendErrorMessage("Failed to disable this jump pad!");
                        }

                        if (_dMgr.RemovePad(_dMgr.DisabledPads.Find(p => p.X == args.TileX && p.Y == args.TileY)))
                            ow.SendSuccessMessage("Successfully enabled this jump pad! (X: {0} Y: {1})", args.TileX,
                                args.TileY);
                        else
                        {
                            ow.SendErrorMessage("Failed to enable this jump pad!");
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TShock.Log.Error($"Exception!\n{e}\n{e.StackTrace}");
            }
            if (_dMgr.UserList.Count(u => u.Uuid == ow.UUID && (u.SelfDisabled || u.DisabledPads.Count(p => p.X == args.TileX && p.Y == args.TileY) != 0)) != 0)
                return;
            if (_dMgr.DisabledPads.Count(p => p.X == args.TileX && p.Y == args.TileY) != 0)
                return;
            ow.TPlayer.velocity.Y = ow.TPlayer.velocity.Y - _config.Height;
            TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", ow.Index);
        }

        private async void CheckUpdates()
        {
            try
            {
                var wClient = new WebClient();
                await wClient.DownloadFileTaskAsync("https://raw.githubusercontent.com/MineBartekSA/TerraJump/master/version.txt", TShock.SavePath + @"\Ver.txt");
            }
            catch (WebException)
            {
                TShock.Log.ConsoleError("[TerraJump Update] Sorry I can't check for updates. Please check your internet connection");
                TShock.Log.Error("Unable to check for updates!");
                return;
            }

            var readFile = new StreamReader(TShock.SavePath + @"\Ver.txt");
            getVer = readFile.ReadLine();
            readFile.Close();
            if (getVer == null || getVer.Length != 5)
            {
                TShock.Log.Error("Invalid Ver.txt file!");
                return;
            }
            var gVer = new Version(getVer[0] - 48, getVer[2] - 48, getVer[4] - 48);

            if (gVer > ver)
            {
                TShock.Log.ConsoleInfo("[TerraJump Update] There is a new version! New version " + gVer + "!");
                updates = gVer;
                return;
            }

            File.Delete(TShock.SavePath + @"\Ver.txt");
            TShock.Log.Info("No Updates");
        }
    }

}
