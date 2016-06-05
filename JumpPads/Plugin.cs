using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace JumpPads
{
    [ApiVersion(1, 23)]
    public class JumpPads : TerrariaPlugin
    {
        //Strings, ints, bools

        private bool itsconfig = false;
        private TSPlayer play;
        private static Timer updateTimer;
        private string _jump = Path.Combine(TShock.SavePath, "Jump.txt");
        private string _configFilePath = Path.Combine(TShock.SavePath, "JumpPads.json");
        private bool toogleJumpPads = true;
        private string JBID = "193";
        private int height = 500000;

        //End of this :D
        //Load stage
        public override string Name
        {
            get { return "JumpPads"; }
        }
        public override Version Version
        {
            get { return new Version(1, 0, 0); }
        }
        public override string Author
        {
            get { return "MineBartekSA"; }
        }
        public override string Description
        {
            get { return "It's simple JumpPads plugn for Tshock!"; }
        }
        public JumpPads(Main game) : base(game)
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
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            //ServerApi.Hooks.ProjectileTriggerPressurePlate
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //UnHooks
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
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
                reader = reader.Replace("ToggleJumpPads = ", "");
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
                TShock.Log.Info("Toogle JumpPads = " + toogleJumpPads);
                //TShock.Log.Info("JumpPadsBlock = " + JBID);
                TShock.Log.Info("Height = " + height);
                //End of Load config
            }

            else if(!itsconfig)
            {
                //Creating config
                TShock.Log.Info("Creating Config");
                StreamWriter sw = new StreamWriter(File.Create(path));
                sw.WriteLine("ToogleJumpPads = " + toogleJumpPads);
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
            Commands.ChatCommands.Add(new Command("jumppads.admin.toggle", toggleJP, "jptoggle", "jpt")
            {
                HelpText = "Turns on/off JumpPads."
            });
            /*Commands.ChatCommands.Add(new Command("jumppads.admin.edit", editJPB, "jpblock")
            {
                HelpText = "Edit block of JumpPdas."
            });*/
            Commands.ChatCommands.Add(new Command("jumppads.admin.editH", editH, "jpheight", "jph")
            {
                HelpText = "Edit height of jump."
            });
            Commands.ChatCommands.Add(new Command("jumppads.admin.reload", reload, "jpreload", "jpr")
            {
                HelpText = "Reload config."
            });
            Commands.ChatCommands.Add(new Command("jumppads.use", runPlayerUpdate, "jump", "j")
            { 
                HelpText = "Jump command!"
            });
            Commands.ChatCommands.Add(new Command("", Info, "jumppads", "jp")
            {
                HelpText = "Information of JumpPads"
            });
        }
        //End Command void
        //Commands execute voids
        void toggleJP(CommandArgs args)
        {
            toogleJumpPads = !toogleJumpPads;
            //Saving changes
            StreamWriter sw = new StreamWriter(File.Create(_configFilePath));
            sw.WriteLine("ToogleJumpPads = " + toogleJumpPads);
            //sw.WriteLine("JupmPadsBlock = " + JBID);
            sw.WriteLine("Height = " + height);
            sw.Close();
            //End of saving
            TShock.Log.ConsoleInfo(args.Player.Name + " toggle JumpPads");
            args.Player.SendSuccessMessage("Succes of toggleing JumpPads. Now is {0}",
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
            sw.WriteLine("ToogleJumpPads = " + toogleJumpPads);
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
            updateTimer.Start();
        }
        void Info (CommandArgs args)
        {
            args.Player.SendInfoMessage("Now height is" + height ,
                                        "To change height use /jpheight <block> or /jph <block>" ,
                                        "Now JumpPads are enable : " + toogleJumpPads ,
                                        "To toggle JumpPads use /jptoggle or /jpt" ,
                                        "To jump use /jump or /j");
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
        void OnPostInitialize(EventArgs args)
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
        }
    }

}
