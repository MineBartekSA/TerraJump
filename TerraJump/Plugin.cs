using System;
using System.Threading;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using System.Collections.Generic;

namespace TerraJump
{
    [ApiVersion(1, 23)]
    public class TerraJump : TerrariaPlugin
    {
        //Strings, ints, bools

        private bool itsconfig = false;
        private TSPlayer play;
        private static System.Timers.Timer updateTimer;
        private string _jump = Path.Combine(TShock.SavePath, "Jump.txt");
        private string _configFilePath = Path.Combine(TShock.SavePath, "TerraJump.json");
        private bool toogleJumpPads = true;
        private string JBID = "193";
        private int height = 20;
        private string ver = "1.1.0";

        //End of this :D
        //Load stage
        public override string Name
        {
            get { return "TerraJump"; }
        }
        public override Version Version
        {
            get { return new Version(1, 1, 0); } // Pamiętaj by zmienić w kilku miejscach =P
        }
        public override string Author
        {
            get { return "MineBartekSA"; }
        }
        public override string Description
        {
            get { return "It's simple JumpPads plugn for Tshock!"; }
        }
        public TerraJump(Main game) : base(game)
        {
            //Nothing!
        }
        public override void Initialize()
        {
            //throw new System.NotImplementedException();
            //Loading configs
            checkConfigFile(_configFilePath);
            load_createConfigFile(_configFilePath);
            //Hooks
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.PlayerTriggerPressurePlate.Register(this, OnTriggerPressurePlate);
            //ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            //ServerApi.Hooks.ProjectileTriggerPressurePlate
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //UnHooks
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                //ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
                ServerApi.Hooks.PlayerTriggerPressurePlate.Deregister(this, OnTriggerPressurePlate);
            }
            base.Dispose(disposing);
        }
        //End Load stage
        //Voids
        //Configs voids
        void checkConfigFile(string path)
        {
            if (File.Exists(path))
            {
                itsconfig = true;
                TShock.Log.Info("Config File exist!");
            }
            
            else
            {
                itsconfig = false;
                TShock.Log.Error("Config File not exist!");
            }
        }
        void load_createConfigFile(string path)
        {
            if(itsconfig)
            {
                //Load config
                TShock.Log.Info("Starting Loading Config");
                StreamReader sr = new StreamReader(File.OpenRead(path));
                //Read Toggle
                var reader = sr.ReadLine();
                reader = reader.Replace("ToggleTerraJump = ", "");
                if (reader == "true")
                    toogleJumpPads = true;
                else if (reader == "false")
                    toogleJumpPads = false;
                //Read JumpPadsBlock
                //reader = sr.ReadLine();
                //reader = reader.Replace("JumpPadsBlock = ", "");
                //JBID = reader;
                //Read Height
                reader = sr.ReadLine();
                reader = reader.Replace("Height = ", "");
                height = Int32.Parse(reader);
                //End Read
                sr.Close();
                TShock.Log.Info("Loading Config Complited!");
                TShock.Log.Info("Toogle TerraJump = " + toogleJumpPads);
                //TShock.Log.Info("JumpPadsBlock = " + JBID);
                TShock.Log.Info("Height = " + height);
                //End of Load config
            }

            else if(!itsconfig)
            {
                //Creating config
                TShock.Log.Info("Creating Config");
                StreamWriter sw = new StreamWriter(File.Create(path));
                sw.WriteLine("ToogleTerraJump = " + toogleJumpPads);
                //sw.WriteLine("JupmPadsBlock = " + JBID);
                sw.WriteLine("Height = " + height);
                sw.Close();
                TShock.Log.Info("Config File created!");
                //End of Creating Config
            }
        }
        //End of configs voids
        //Commmands void
        void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("terrajump.admin.toggle", toggleJP, "tjtoggle", "tjt")
            {
                HelpText = "Turns on/off TerraJump."
            });
            /*Commands.ChatCommands.Add(new Command("terrajump.admin.edit", editJPB, "tjblock")
            {
                HelpText = "Edit block of JumpPdas."
            });*/
            Commands.ChatCommands.Add(new Command("terrajump.admin.editH", editH, "tjheight", "tjh")
            {
                HelpText = "Edit height of jump."
            });
            Commands.ChatCommands.Add(new Command("terrajump.admin.reload", reload, "tjreload", "tjr")
            {
                HelpText = "Reload config."
            });
            Commands.ChatCommands.Add(new Command("terrajump.use", runPlayerUpdate, "jump", "j")
            { 
                HelpText = "Jump command!"
            });
            Commands.ChatCommands.Add(new Command("", Info, "terrajump", "jp")
            {
                HelpText = "Information of TerraJump"
            });
            Commands.ChatCommands.Add(new Command("", skyJump, "spacelaunch", "sl")
            {
                HelpText = "Launch your victim in to space!"
            });
        }
        //End Command void
        //Commands execute voids
        void toggleJP(CommandArgs args)
        {
            toogleJumpPads = !toogleJumpPads;
            //Saving changes
            StreamWriter sw = new StreamWriter(File.Create(_configFilePath));
            sw.WriteLine("ToogleTerraJump = " + toogleJumpPads);
            //sw.WriteLine("JupmPadsBlock = " + JBID);
            sw.WriteLine("Height = " + height);
            sw.Close();
            //End of saving
            TShock.Log.ConsoleInfo(args.Player.Name + " toggle TerraJump");
            args.Player.SendSuccessMessage("Succes of toggleing TerraJump. Now is {0}",
                (toogleJumpPads) ? "ON" : "OFF");
        }
        /*void editJPB(CommandArgs args)
        {

        }*/
        void editH(CommandArgs args)
        {
            float a = float.Parse(args.Parameters[0]);
            args.Player.SendInfoMessage("You set height as " + a);
            height = (int)a;
            TShock.Log.ConsoleInfo("Height set as " + a);
            StreamWriter sw = new StreamWriter(File.Create(_configFilePath));
            sw.WriteLine("ToogleTerraJump = " + toogleJumpPads);
            sw.WriteLine("Height = " + height);
            sw.Close();
        }
        void reload(CommandArgs args)
        {
            TShock.Log.ConsoleInfo("Reloading config!");
            checkConfigFile(_configFilePath);
            if (itsconfig)
                load_createConfigFile(_configFilePath);

            else if(!itsconfig)
            {
                args.Player.SendErrorMessage("The config file is missing! So i create them!");
                TShock.Log.ConsoleError("The config file is missing! So i create them!");
                load_createConfigFile(_configFilePath);
            }
            args.Player.SendSuccessMessage("Realod complited!");
            TShock.Log.ConsoleInfo("Reload complited!");
        }
        void runPlayerUpdate(CommandArgs args)
        {
            if (!toogleJumpPads)
                return;
            play = args.Player;
            Thread.Sleep(500);
            play.TPlayer.velocity.Y = play.TPlayer.velocity.Y - height;
            TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", play.Index);
            play.SendInfoMessage("Jump!");
            //updateTimer.Start();
        }
        void Info (CommandArgs args)
        {
            args.Player.SendInfoMessage("TerraJump plugin on version " + ver);
            args.Player.SendInfoMessage("Now height is a " + height);
            args.Player.SendInfoMessage("Now TerraJump are enable : " + toogleJumpPads);
            args.Player.SendInfoMessage("To change height use /tjheight <block> or /tjh <block>");
            args.Player.SendInfoMessage("To toggle TerraJump use /tjtoggle or /tjt");
            args.Player.SendInfoMessage("To jump use /jump or /j");
        }
        void skyJump (CommandArgs args)
        {
            if (args.Player.RealPlayer)
            {
                args.Player.SendErrorMessage("You must run this command from the console.");
                return;
            }
            //TShock.Utils.FindPlayer(args.Parameters[0]); Use this
            foreach(var a in TShock.Utils.FindPlayer(args.Parameters[0]))
            {
                a.TPlayer.velocity.Y = a.TPlayer.velocity.Y - 100;
                a.SendInfoMessage("You have been launch in to space! Hahahahahaha!");
                args.Player.SendInfoMessage(a.ToString() + " is in space now!");
            }
        }
        //End commands ecexute voids
        //Presure Plate trigger
        void OnTriggerPressurePlate(TriggerPressurePlateEventArgs<Player> args)
        {
                var playerPos = args.Object.position;
                Tile pressurePlate = Main.tile[args.TileX, args.TileY];
                Tile underBlock = Main.tile[args.TileX, args.TileY + 1];
        }
        //End presure plate trigger
        //Other
        /*void OnPostInitialize(EventArgs args)
        {
            updateTimer = new Timer(500);
            updateTimer.Elapsed += UpdateTimerOnElapsed;
        }
        void UpdateTimerOnElapsed(object sender, ElapsedEventArgs args)
        {
            play.TPlayer.velocity.Y = play.TPlayer.velocity.Y - height;
            TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", play.Index);
            play.SendInfoMessage("Jump!");
            updateTimer.Stop();
        }*/
    }

}
