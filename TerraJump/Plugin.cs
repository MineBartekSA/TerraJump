using System;
using System.Threading;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace TerraJump
{
    [ApiVersion(1, 23)]
    public class TerraJump : TerrariaPlugin
    {
        //Strings, ints, bools

        private bool itsconfig = false;
        private TSPlayer play;
        private string _configFilePath = Path.Combine(TShock.SavePath, "TerraJump.json");
        private bool toogleJumpPads = true;
       // private string JBID = "193";
        private int height = 20;
        private string ver = "1.2.0";
        private bool projectileTriggerEnable;

        //End of this :D
        //Load stage
        public override string Name
        {
            get { return "TerraJump"; }
        }
        public override Version Version
        {
            get { return new Version(1, 2, 0); } // Pamiętaj by zmienić w kilku miejscach =P
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
            ServerApi.Hooks.PlayerTriggerPressurePlate.Register(this, OnPlayerTriggerPressurePlate);
            ServerApi.Hooks.ProjectileTriggerPressurePlate.Register(this, OnProjectileTriggerPressurePlate);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //UnHooks
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.PlayerTriggerPressurePlate.Deregister(this, OnPlayerTriggerPressurePlate);
                ServerApi.Hooks.ProjectileTriggerPressurePlate.Deregister(this, OnProjectileTriggerPressurePlate);
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
                HelpText = "Turns on/off TerraJump"
            });
            /*Commands.ChatCommands.Add(new Command("terrajump.admin.edit", editJPB, "tjblock")
            {
                HelpText = "Edit block of JumpPdas."
            });*/
            Commands.ChatCommands.Add(new Command("terrajump.admin.editH", editH, "tjheight", "tjh")
            {
                HelpText = "Edit height of jump"
            });
            Commands.ChatCommands.Add(new Command("terrajump.admin.reload", reload, "tjreload", "tjr")
            {
                HelpText = "Reload config"
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
            Commands.ChatCommands.Add(new Command("terrajump.admin.projectileToggle", projTog, "tjprojectiletoggle", "tjpt")
            {
                HelpText = "Turns on/off TerraJump projectile jumps"
            });
            Commands.ChatCommands.Add(new Command("", re, "re", "reverse")
            {
                HelpText = "For fun! It reverse text and send it!"
            });
            //For Dev! Only!
            // /*
            Commands.ChatCommands.Add(new Command("terrajump.dev", y, "gety", "gy", "y")
            {
                HelpText = "Only for dev!"
            });
            // */
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
        void projTog(CommandArgs args)
        {
            projectileTriggerEnable = !projectileTriggerEnable;
            TShock.Log.ConsoleInfo(args.Player.Name + " toggle TerraJump");
            args.Player.SendSuccessMessage("Succes of toggleing projectile jumps. Now is {0}",
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
            args.Player.SendInfoMessage("Now projectile jumos are enable : " + projectileTriggerEnable);
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
            foreach(TSPlayer a in TShock.Utils.FindPlayer(args.Parameters[0]))
            {
                //TP to surface
                up(args.Player);
                //Jump
                Thread.Sleep(100);
                a.TPlayer.velocity.Y = a.TPlayer.velocity.Y - 1000;
                TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", a.Index);
                /*Thread.Sleep(200);
                a.TPlayer.velocity.Y = a.TPlayer.velocity.Y - 1000;
                TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", a.Index);*/
                a.SendInfoMessage("You have been launch in to space! Hahahahahaha!");
                args.Player.SendInfoMessage(a.Name + " is in space now!");
            }
        }
        void y(CommandArgs arg)
        {
            float y = arg.TPlayer.position.Y;
            arg.Player.SendInfoMessage("Your Y posision is now : " + y);
        }
        void re(CommandArgs arg)
        {
            string par = "";
            string rew = "";
            int list = arg.Parameters.Count;
            for (int i = list - 1; i >= 0; i--)
            {
                string word = arg.Parameters[i];
                for (int ii = word.Length - 1; ii >= 0; ii--)
                    rew += word[i];
                par += rew;
                rew = "";
            }
            arg.Player.SendMessageFromPlayer(par,255,255,255,1);
        }
        //End commands ecexute voids



        //Presure Plate trigger void
        void OnPlayerTriggerPressurePlate(TriggerPressurePlateEventArgs<Player> args)
        {
            if (!projectileTriggerEnable)
                return;
            TShock.Log.ConsoleInfo("[PlTPP]Starting procedure");
            bool pds = false;
            TSPlayer ow = TShock.Players[args.Object.whoAmI];
            Tile pressurePlate = Main.tile[args.TileX, args.TileY];
            Tile underBlock = Main.tile[args.TileX, args.TileY + 1];
            Tile upBlock = Main.tile[args.TileX, args.TileY - 1];
            Tile leftBlock = Main.tile[args.TileX + 1, args.TileY];
            Tile rightBlock = Main.tile[args.TileX - 1, args.TileY];
            if (underBlock.type == Terraria.ID.TileID.SlimeBlock)
            {
                TShock.Log.ConsoleInfo("[PlTPP]O on 'Under' this slime block are!");
                bool ulb = false;
                bool urb = false;
                Tile underLeftBlock = Main.tile[args.TileX + 1, args.TileY + 1];
                Tile underRightBlock = Main.tile[args.TileX - 1, args.TileY + 1];
                if (underLeftBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PlTPP]Ok on left!");
                    ulb = true;
                }
                if (underRightBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PlTPP]Ok on right!");
                    urb = true;
                }
                if (ulb && urb)
                    pds = true;
                else
                    TShock.Log.ConsoleInfo("There is one or two slime blocks but i need three! Stoping");
            }
            else if (upBlock.type == Terraria.ID.TileID.SlimeBlock)
            {
                TShock.Log.ConsoleInfo("[PlTPP]O on 'Up' this slime block are!");
                bool ulb = false;
                bool urb = false;
                Tile upLeftBlock = Main.tile[args.TileX + 1, args.TileY - 1];
                Tile upRightBlock = Main.tile[args.TileX - 1, args.TileY - 1];
                if (upLeftBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PlTPP]Ok on left!");
                    ulb = true;
                }
                if (upRightBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PlTPP]Ok on right!");
                    urb = true;
                }
                if (ulb && urb)
                    pds = true;
                else
                    TShock.Log.ConsoleInfo("There is one or two slime blocks but i need three! Stoping");
            }
            else if (leftBlock.type == Terraria.ID.TileID.SlimeBlock)
            {
                TShock.Log.ConsoleInfo("[PlTPP]O on 'Left' this slime block are!");
                bool ulb = false;
                bool urb = false;
                Tile leftUpBlock = Main.tile[args.TileX + 1, args.TileY - 1];
                Tile leftUnderBlock = Main.tile[args.TileX + 1, args.TileY + 1];
                if (leftUpBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PlTPP]Ok on up!");
                    ulb = true;
                }
                if (leftUnderBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PlTPP]Ok on under!");
                    urb = true;
                }
                if (ulb && urb)
                    pds = true;
                else
                    TShock.Log.ConsoleInfo("There is one or two slime blocks but i need three! Stoping");
            }
            else if (rightBlock.type == Terraria.ID.TileID.SlimeBlock)
            {
                TShock.Log.ConsoleInfo("[PlTPP]O on 'Right' thsi slime block are!");
                bool ulb = false;
                bool urb = false;
                Tile rightUpBlock = Main.tile[args.TileX - 1, args.TileY - 1];
                Tile rightUnderBlock = Main.tile[args.TileX - 1, args.TileY + 1];
                if (rightUpBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PlTPP]Ok on up!");
                    ulb = true;
                }
                if (rightUnderBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PlTPP]Ok on under!");
                    urb = true;
                }
                if (ulb && urb)
                    pds = true;
                else
                    TShock.Log.ConsoleInfo("There is one or two slime blocks but i need three! Stoping");
            }
            else
            {
                TShock.Log.ConsoleInfo("[PlTPP]Can't find any SlimeBlocks! Stoping");
                return;
            }

            if (pds)
            {
                ow.TPlayer.velocity.Y = ow.TPlayer.velocity.Y - height;
                TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", ow.Index);
                TShock.Log.ConsoleInfo("[PlTPP]Wooh! Procedure succesfull finish!");
                ow.SendInfoMessage("Jump!");
            }
        }
        //End presure plate trigger void

        
        
        //Projectile triggered pressure plate VOID
        void OnProjectileTriggerPressurePlate(TriggerPressurePlateEventArgs<Projectile> args)
        {
            if (!projectileTriggerEnable)
                return;
            TShock.Log.ConsoleInfo("[PTPP]Starting procedure");
            bool pds = false;
            TSPlayer ow = TShock.Players[args.Object.owner];
            Tile pressurePlate = Main.tile[args.TileX, args.TileY];
            Tile underBlock = Main.tile[args.TileX, args.TileY + 1];
            Tile upBlock = Main.tile[args.TileX, args.TileY - 1];
            Tile leftBlock = Main.tile[args.TileX + 1, args.TileY];
            Tile rightBlock = Main.tile[args.TileX - 1, args.TileY];
            if (underBlock.type == Terraria.ID.TileID.SlimeBlock)
            {
                TShock.Log.ConsoleInfo("[PTPP]O on 'Under' this slime block are!");
                bool ulb = false;
                bool urb  = false;
                Tile underLeftBlock = Main.tile[args.TileX + 1, args.TileY + 1];
                Tile underRightBlock = Main.tile[args.TileX - 1, args.TileY + 1];
                if (underLeftBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PTPP]Ok on left!");
                    ulb = true;
                }
                if (underRightBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PTPP]Ok on right!");
                    urb = true;
                } 
                if (ulb && urb)
                    pds = true;
                else
                    TShock.Log.ConsoleInfo("There is one or two slime blocks but i need three! Stoping");
            }
            else if (upBlock.type == Terraria.ID.TileID.SlimeBlock)
            {
                TShock.Log.ConsoleInfo("[PTPP]O on 'Up' this slime block are!");
                bool ulb = false;
                bool urb = false;
                Tile upLeftBlock = Main.tile[args.TileX + 1, args.TileY - 1];
                Tile upRightBlock = Main.tile[args.TileX - 1, args.TileY - 1];
                if (upLeftBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PTPP]Ok on left!");
                    ulb = true;
                }
                if (upRightBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PTPP]Ok on right!");
                    urb = true;
                }
                if (ulb && urb)
                    pds = true;
                else
                    TShock.Log.ConsoleInfo("There is one or two slime blocks but i need three! Stoping");
            }
            else if (leftBlock.type == Terraria.ID.TileID.SlimeBlock)
            {
                TShock.Log.ConsoleInfo("[PTPP]O on 'Left' this slime block are!");
                bool ulb = false;
                bool urb = false;
                Tile leftUpBlock = Main.tile[args.TileX + 1, args.TileY - 1];
                Tile leftUnderBlock = Main.tile[args.TileX + 1, args.TileY + 1];
                if (leftUpBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PTPP]Ok on up!");
                    ulb = true;
                }
                if (leftUnderBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PTPP]Ok on under!");
                    urb = true;
                }
                if (ulb && urb)
                    pds = true;
                else
                    TShock.Log.ConsoleInfo("There is one or two slime blocks but i need three! Stoping");
            }
            else if (rightBlock.type == Terraria.ID.TileID.SlimeBlock)
            {
                TShock.Log.ConsoleInfo("[PTPP]O on 'Right' thsi slime block are!");
                bool ulb = false;
                bool urb = false;
                Tile rightUpBlock = Main.tile[args.TileX - 1, args.TileY - 1];
                Tile rightUnderBlock = Main.tile[args.TileX - 1, args.TileY + 1];
                if (rightUpBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PTPP]Ok on up!");
                    ulb = true;
                }
                if (rightUnderBlock.type == Terraria.ID.TileID.SlimeBlock)
                {
                    TShock.Log.ConsoleInfo("[PTPP]Ok on under!");
                    urb = true;
                }
                if (ulb && urb)
                    pds = true;
                else
                    TShock.Log.ConsoleInfo("There is one or two slime blocks but i need three! Stoping");
            }
            else
            {
                TShock.Log.ConsoleInfo("[PTPP]Can't find any SlimeBlocks! Stoping");
                return;
            }

            if (pds)
            {
                ow.TPlayer.velocity.Y = ow.TPlayer.velocity.Y - height;
                TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", ow.Index);
                TShock.Log.ConsoleInfo("[PTPP]Wooh! Procedure succesfull finish!");
                ow.SendInfoMessage("Jump!");
            }
        }
        //End projectile triggered pressure plate VOID

        
        
        //Other Voids
        void up (TSPlayer player)
        {
            float x = player.TPlayer.position.X;
            float pos = player.TPlayer.position.Y;
            if(pos < 5478)
            {
                player.Teleport(x, 5478);
            }
            else if (pos >= 5478)
            {
                return;
            }

        }
        /*void OnPostInitialize(EventArgs args)    // Old metod
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
        //End fo Voids
    }

}
