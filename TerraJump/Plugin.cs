﻿using System;
using System.Threading;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using System.Net;

namespace TerraJump
{
    [ApiVersion(1, 25)]
    public class TerraJump : TerrariaPlugin
    {
        //Strings, ints, bools
        private TSPlayer play;
        private string _configFilePath = Path.Combine(TShock.SavePath, "TerraJump.json");
        private static Config conf;
        private string ver = "2.0.4"; // Pamiętaj by zmienić w kilku miejscach =P
        public string constr;
        //Configs
        private bool toggleJumpPads;
        private int JBID;
        private int height;
        private bool projectileTriggerEnable;
        private bool pressureTriggerEnable;
        private string reFormat;
        private byte red;
        private byte green;
        private byte blue;

        //End of this :D
        //Load stage
        public override string Name
        {
            get { return "TerraJump"; }
        }
        public override Version Version
        {
            get { return new Version(2, 0, 4); } // Pamiętaj by zmienić w kilku miejscach =P
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
            //Loading configs
            cUP();
            conf = Config.loadProcedure(_configFilePath);
            toggleJumpPads = conf.toggleJumpPads;
            height = conf.height;
            JBID = conf.JBID;
            pressureTriggerEnable = conf.pressureTriggerEnable;
            projectileTriggerEnable = conf.projectileTriggerEnable;
            reFormat = conf.reFormat;
            red = conf.ReRed;
            green = conf.ReGrean;
            blue = conf.ReBlue;
            //Hooks
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.PlayerTriggerPressurePlate.Register(this, OnPlayerTriggerPressurePlate);
            //TShock.Log.ConsoleInfo("Hey listen!!! " + slimeblockID);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //UnHooks
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.PlayerTriggerPressurePlate.Deregister(this, OnPlayerTriggerPressurePlate);
            }
            base.Dispose(disposing);
        }
        //End Load stage



        //Voids



        //Commmands void
        void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("terrajump.admin.toggle", toggleJP, "tjtoggle", "tjt")
            {
                HelpText = "Turns on/off TerraJump"
            });
            Commands.ChatCommands.Add(new Command("terrajump.admin.edit", editJPB, "tjblock", "tjb")
            {
                HelpText = "Edit block of JumpPdas."
            });
            Commands.ChatCommands.Add(new Command("terrajump.admin.editH", editH, "tjheight", "tjh")
            {
                HelpText = "Edit height of jump"
            });
            /*Commands.ChatCommands.Add(new Command("terrajump.admin.reload", reload, "tjreload", "tjr")
            {
                HelpText = "Reload config"
            });*/
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
                HelpText = "Launch your victim in to space! At your risk!!"
            });
            Commands.ChatCommands.Add(new Command("terrajump.admin.pressuretoggle", JPTog, "tjpressuretoggle", "tjpt")
            {
                HelpText = "Turns on/off TerraJump pressure plate jumps"
            });
            Commands.ChatCommands.Add(new Command("", re, "re", "reverse")
            {
                HelpText = "For fun! It reverse text and send it!"
            });
            //For Dev! Only!
             /*
            Commands.ChatCommands.Add(new Command("terrajump.dev", y, "gety", "gy", "y")
            {
                HelpText = "Only for dev!"
            });
             */
        }
        //End Command void



        //Commands execute voids
        void toggleJP(CommandArgs args)
        {
            toggleJumpPads = !toggleJumpPads;
            //Saving changes
            conf = Config.update(_configFilePath, toggleJumpPads, height, JBID, pressureTriggerEnable, projectileTriggerEnable, reFormat, red, green, blue);
            //End of saving
            TShock.Log.ConsoleInfo(args.Player.Name + " toggle TerraJump");
            args.Player.SendSuccessMessage("Succes of toggleing TerraJump. Now is {0}",
                (toggleJumpPads) ? "ON" : "OFF");
        }
        void JPTog(CommandArgs args)
        {
            pressureTriggerEnable = !pressureTriggerEnable;
            conf = Config.update(_configFilePath, toggleJumpPads, height, JBID, pressureTriggerEnable, projectileTriggerEnable, reFormat, red, green, blue);
            TShock.Log.ConsoleInfo(args.Player.Name + " toggle TerraJump");
            args.Player.SendSuccessMessage("Succes of toggleing JumpPads. Now is {0}",
                (pressureTriggerEnable) ? "ON" : "OFF");
        }
        void editH(CommandArgs args)
        {
            float a = float.Parse(args.Parameters[0]);
            args.Player.SendInfoMessage("You set height as " + a);
            height = (int)a;
            TShock.Log.ConsoleInfo("Height set as " + a);
            conf = Config.update(_configFilePath, toggleJumpPads, height, JBID, pressureTriggerEnable, projectileTriggerEnable, reFormat, red, green, blue);
        }
        void reload(CommandArgs args)
        {
            conf = Config.loadProcedure(_configFilePath);
            args.Player.SendInfoMessage("Reload complited!");
        }
        void runPlayerUpdate(CommandArgs args)
        {
            if (!toggleJumpPads)
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
            args.Player.SendInfoMessage("Now TerraJump are enable : " + toggleJumpPads);
            args.Player.SendInfoMessage("Now pressure jumps are enable : " + pressureTriggerEnable);
            args.Player.SendInfoMessage("To toggle JumpPads use /tjpressuretoggle or /tjpt");
            args.Player.SendInfoMessage("To change height use /tjheight <block> or /tjh <block>");
            args.Player.SendInfoMessage("To change JumpPads block use /tjblock <title ID> or /tjb <Title ID>");
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
            char[] wleng;
            int list = arg.Parameters.Count;
            TShock.Log.Info("You have created a " + list + " parametrs!");
            for (int i = list - 1; i >= 0; i--)
            {
                string word = arg.Parameters[i];
                TShock.Log.Info("Word for now is " + word + "!");
                wleng = word.ToCharArray();
                TShock.Log.Info("World have " + wleng + " characters!");
                Array.Reverse(wleng);
                rew = new string(wleng);
                TShock.Log.Info("Reversing complete! Now is " + rew + "!");
                par += " ";
                par += rew;
                rew = "";
            }
            TShock.Log.Info("Reversing all world complete!");
            TShock.Log.Info("Now this is " + par);
            string message = formatRe(par, reFormat, arg.Player);
            TShock.Log.Info("Not before formating is " + message);
            arg.Player.SendMessageFromPlayer(message,red,green,blue,1);
            TShock.Log.Info("Reversing sending complete!");
        }
        void editJPB(CommandArgs args)
        {
            float a = float.Parse(args.Parameters[0]);
            args.Player.SendInfoMessage("You set block ID as " + a);
            JBID = (int)a;
            TShock.Log.ConsoleInfo("Block ID set as " + a);
            conf = Config.update(_configFilePath, toggleJumpPads, height, JBID, pressureTriggerEnable, projectileTriggerEnable, reFormat, red, green, blue);
        }
        //End commands ecexute voids



        //Presure Plate trigger void
        void OnPlayerTriggerPressurePlate(TriggerPressurePlateEventArgs<Player> args)
        {
            if (!pressureTriggerEnable)
                return;
            else if (!TShock.Players[args.Object.whoAmI].HasPermission("terrajump.usepad"))
                return;
            //TShock.Log.ConsoleInfo("[PlTPP]Starting procedure");
            bool pds = false;
            TSPlayer ow = TShock.Players[args.Object.whoAmI];
            Tile pressurePlate = Main.tile[args.TileX, args.TileY];
            Tile underBlock = Main.tile[args.TileX, args.TileY + 1];
            Tile upBlock = Main.tile[args.TileX, args.TileY - 1];
            if (underBlock.type == JBID)
            {
                //TShock.Log.ConsoleInfo("[PlTPP]O on 'Under' this slime block are!");
                bool ulb = false;
                bool urb = false;
                Tile underLeftBlock = Main.tile[args.TileX - 1, args.TileY + 1];
                Tile underRightBlock = Main.tile[args.TileX + 1, args.TileY + 1];
                if (underLeftBlock.type == JBID)
                {
                    //TShock.Log.ConsoleInfo("[PlTPP]Ok on left!");
                    ulb = true;
                }
                if (underRightBlock.type == JBID)
                {
                    //TShock.Log.ConsoleInfo("[PlTPP]Ok on right!");
                    urb = true;
                }
                if (ulb && urb)
                    pds = true;
            }
            else if (upBlock.type == JBID)
            {
                //TShock.Log.ConsoleInfo("[PlTPP]O on 'Up' this slime block are!");
                bool ulb = false;
                bool urb = false;
                Tile upLeftBlock = Main.tile[args.TileX - 1, args.TileY - 1];
                Tile upRightBlock = Main.tile[args.TileX + 1, args.TileY - 1];
                if (upLeftBlock.type == JBID)
                {
                    //TShock.Log.ConsoleInfo("[PlTPP]Ok on left!");
                    ulb = true;
                }
                if (upRightBlock.type == JBID)
                {
                    //TShock.Log.ConsoleInfo("[PlTPP]Ok on right!");
                    urb = true;
                }
                if (ulb && urb)
                    pds = true;
            }
            else
            {
                //TShock.Log.ConsoleInfo("[PlTPP]Can't find any SlimeBlocks! Stoping");
                return;
            }

            if (pds)
            { 
                ow.TPlayer.velocity.Y = ow.TPlayer.velocity.Y - height;
                TSPlayer.All.SendData(PacketTypes.PlayerUpdate, "", ow.Index);
                //TShock.Log.ConsoleInfo("[PlTPP]Wooh! Procedure succesfull finish!");
                ow.SendInfoMessage("Jump!");
            }
        }
        //End presure plate trigger void



        //Chceck Update VOID
        void cUP()
        {
            try
            {
                WebClient WClient = new WebClient();
                WClient.DownloadFile("https://raw.githubusercontent.com/MineBartekSA/TerraJump/master/version.txt", TShock.SavePath + @"\Ver.txt");
            }
            catch (WebException)
            {
                TShock.Log.ConsoleError("[TerraJump Update] Sorry i can't check updates. Please chcek your internet connection");
                TShock.Log.Error("Sorry i can't check updates. Please chcek your internet connection");
                return;
            }

            StreamReader readfile = new StreamReader(TShock.SavePath + @"\Ver.txt");
            string getver =  readfile.ReadLine();
            readfile.Close();

            int firstGetver = (int)getver[0];
            int secondGetver = (int)getver[2];
            int thirdGetver = (int)getver[4];

            int firstver = (int)ver[0];
            int secondver = (int)ver[2];
            int thirdver = (int)ver[4];

            /*TShock.Log.ConsoleInfo("Fisrt get " + firstGetver + " Second get " + secondGetver + " Third get " + thirdGetver);        // FOR DEVS ONLY!!
            TShock.Log.ConsoleInfo("Fisrt ver " + firstver + " Second ver " + secondver + " Third ver " + thirdver);*/

            if (firstGetver > firstver)
            {
                TShock.Log.ConsoleInfo("[TerraJump Update] There is a new version! New version " + getver + "!");
                return;
            }

            if (secondGetver > secondver)
            {
                TShock.Log.ConsoleInfo("[TerraJump Update] There is a new version! New version " + getver + "!");
                return;
            }

            if (thirdGetver > thirdver)
            {
                TShock.Log.ConsoleInfo("[TerraJump Update] There is a new version! New version " + getver + "!");
                return;
            }

            File.Delete(TShock.SavePath + @"\Ver.txt");

            return;
        }
        //End do Check Update VOID

        
        
        //Format reverse VOID
        string formatRe(string mess, string formstring, TSPlayer arg)
        {
            string format = "";

            format = formstring.Replace(":group:", arg.Group.Name).Replace(":user:", arg.Name).Replace(":mess:", mess);

            return format;
        }
        //End Format reverso VOID



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

        //End fo Voids
    }

}