using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Data;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using System.Net;

namespace TerraJump
{
    [ApiVersion(1, 26)]
    public class TerraJump : TerrariaPlugin
    {
        //Strings, ints, bools
        private TSPlayer play;
        private string _configFilePath = Path.Combine(TShock.SavePath, "TerraJump.json");
        private static Config conf;
        private static TJUDis UDis;
        private string ver = "2.1.2"; // Pamiętaj by zmienić w kilku miejscach =P
        public string constr;
        private bool isUpdates;
        private string getver;
        private List<TSPlayer> isDisabling = new List<TSPlayer>();
        //Configs
        private bool toggleJumpPads;
        private int JBID;
        private int height;
        private bool pressureTriggerEnable;
        private string reFormat;
        private byte red;
        private byte green;
        private byte blue;
        private List<string> userlist;
        private DataSet XYSet;

        //End of this :D
        //Load stage
        public override string Name
        {
            get { return "TerraJump"; }
        }
        public override Version Version
        {
            get { return new Version(2, 1, 2); } // Pamiętaj by zmienić w kilku miejscach =P
        }
        public override string Author
        {
            get { return "MineBartekSA"; }
        }
        public override string Description
        {
            get { return "It's simple JumpPads plugin for Tshock!"; }
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
            UDis = TJUDis.start();
            TShock.Log.Info("Finish of Config loading and starting loading plugin");
            toggleJumpPads = conf.toggleJumpPads;
            if(toggleJumpPads == false)
            {
                TShock.Log.ConsoleError("You need to have a good config!");
                return;
            }
            height = conf.height;
            JBID = conf.JBID;
            pressureTriggerEnable = conf.pressureTriggerEnable;
            reFormat = conf.reFormat;
            red = conf.ReRed;
            green = conf.ReGrean;
            blue = conf.ReBlue;
            userlist = UDis.UList;
            XYSet = UDis.XYSet;
            //Hooks
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.PlayerTriggerPressurePlate.Register(this, OnPlayerTriggerPressurePlate);
            TShock.Log.Info("Init complate!");
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
            Commands.ChatCommands.Add(new Command(Info, "terrajump", "jp")
            {
                HelpText = "Information of TerraJump"
            });
            Commands.ChatCommands.Add(new Command(skyJump, "spacelaunch", "sl")
            {
                HelpText = "Launch your victim in to space! At your risk!!"
            });
            Commands.ChatCommands.Add(new Command("terrajump.admin.pressuretoggle", JPTog, "tjpressuretoggle", "tjpt")
            {
                HelpText = "Turns on/off TerraJump pressure plate jumps"
            });
            Commands.ChatCommands.Add(new Command(re, "re", "reverse")
            {
                HelpText = "For fun! It reverse text and send it!"
            });
            Commands.ChatCommands.Add(new Command("terrajump.disable", TJDis, "tjdisable", "tjd")
            {
                HelpText = "Disable or enable only for you TerraJump"
            });
            Commands.ChatCommands.Add(new Command("terrajump.admin.disablejumppad", JPDis, "tjpaddisable", "tjpd")
            {
                HelpText = "Disable or enable a indicated JumpPad"
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
            conf = Config.update(_configFilePath, toggleJumpPads, height, JBID, pressureTriggerEnable, reFormat, red, green, blue);
            //End of saving
            TShock.Log.ConsoleInfo(args.Player.Name + " toggle TerraJump");
            args.Player.SendSuccessMessage("Succes of toggleing TerraJump. Now is {0}",
                (toggleJumpPads) ? "ON" : "OFF");
        }
        void JPTog(CommandArgs args)
        {
            if (toggleJumpPads == false)
            {
                TShock.Log.ConsoleError("You need to turn on your plugin!");
                return;
            }
            pressureTriggerEnable = !pressureTriggerEnable;
            conf = Config.update(_configFilePath, toggleJumpPads, height, JBID, pressureTriggerEnable, reFormat, red, green, blue);
            TShock.Log.ConsoleInfo(args.Player.Name + " toggle TerraJump");
            args.Player.SendSuccessMessage("Succes of toggleing JumpPads. Now is {0}",
                (pressureTriggerEnable) ? "ON" : "OFF");
        }
        void editH(CommandArgs args)
        {
            if (toggleJumpPads == false)
            {
                TShock.Log.ConsoleError("You need to turn on your plugin!");
                return;
            }
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("You must give a parametr");
                args.Player.SendErrorMessage("Use /tjheight <number>");
            }
            float a = float.Parse(args.Parameters[0]);
            args.Player.SendInfoMessage("You set height as " + a);
            height = (int)a;
            TShock.Log.ConsoleInfo("Height set as " + a);
            conf = Config.update(_configFilePath, toggleJumpPads, height, JBID, pressureTriggerEnable, reFormat, red, green, blue);
        }
        void reload(CommandArgs args)
        {
            // To rewrite!
            args.Player.SendInfoMessage("Reload complited!");
        }
        void runPlayerUpdate(CommandArgs args)
        {
            if (!toggleJumpPads)
                return;
            else if (userlist.Contains(args.Player.Name))
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
            args.Player.SendInfoMessage("Now TerraJump are : {0}", (toggleJumpPads) ? "ON" : "OFF");
            if (isUpdates)
                args.Player.SendInfoMessage("There is new update! Version " + getver);
            args.Player.SendInfoMessage("Now JumpPads are : {0}",(pressureTriggerEnable) ? "ON" : "OFF");
            args.Player.SendInfoMessage("To toggle JumpPads use /tjpressuretoggle or /tjpt");
            args.Player.SendInfoMessage("To change height use /tjheight <block> or /tjh <block>");
            args.Player.SendInfoMessage("To change JumpPads block use /tjblock <title ID> or /tjb <Title ID>");
            args.Player.SendInfoMessage("To disable or enable only for you JumpPads use /tjdisable or /tjd");
            args.Player.SendInfoMessage("To disable or enable a indicated JumpPad use /tjpaddisable or /tjpd");
            args.Player.SendInfoMessage("To toggle TerraJump use /tjtoggle or /tjt");
            args.Player.SendInfoMessage("To jump use /jump or /j");
        }
        void skyJump (CommandArgs args)
        {
            if (toggleJumpPads == false)
            {
                TShock.Log.ConsoleError("You need to turn on your plugin!");
                return;
            }
            if (args.Player.RealPlayer)
            {
                args.Player.SendErrorMessage("You must run this command from the console.");
                return;
            }
            if(args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("You must give a parametr");
                args.Player.SendErrorMessage("Use /spacelunch <player>");
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
            if (toggleJumpPads == false)
            {
                TShock.Log.ConsoleError("You need to turn on your plugin!");
                return;
            }
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
            if (toggleJumpPads == false)
            {
                TShock.Log.ConsoleError("You need to turn on your plugin!");
                return;
            }
            if(args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("No parametr! Use /tjblock <number>");
            }
            float a = float.Parse(args.Parameters[0]);
            args.Player.SendInfoMessage("You set block ID as " + a);
            JBID = (int)a;
            TShock.Log.ConsoleInfo("Block ID set as " + a);
            conf = Config.update(_configFilePath, toggleJumpPads, height, JBID, pressureTriggerEnable, reFormat, red, green, blue);
        }
        void TJDis (CommandArgs args)
        {
            bool ifExist = userlist.Contains(args.Player.Name);

            if(ifExist)
            {
                UDis =  TJUDis.remove(args.Player);
                userlist = UDis.UList;
                args.Player.SendInfoMessage("You can use now JumpPads");
            }
            else if (!ifExist)
            {
                UDis = TJUDis.add(args.Player);
                userlist = UDis.UList;
                args.Player.SendInfoMessage("You can't use now JumpPads");
            }
        }
        void JPDis (CommandArgs args)
        {
            if (userlist.Contains(args.Player.Name))
            {
                args.Player.SendErrorMessage("You must can use JumpPads. Use command /tjdisable");
                return;
            }

            isDisabling.Add(args.Player);
            args.Player.SendInfoMessage("Please stand on JumpPad");
            return;
        }
        void JPDisNext(float x, float y, TSPlayer p)
        {
            TShock.Log.Info("Starting adding to dataset");
            UDis = TJUDis.add(x, y);
            XYSet = UDis.XYSet;
            isDisabling.Remove(p);
            p.SendInfoMessage("This JumpPad is now disable!");
        }
        void JPEnaNext(float x, float y, TSPlayer p)
        {
            TShock.Log.Info("Starting removing from dataset");
            UDis = TJUDis.remove(x, y);
            XYSet = UDis.XYSet;
            isDisabling.Remove(p);
            p.SendInfoMessage("This JumpPad is now enable!");
        }
        //End commands ecexute voids



        //Presure Plate trigger void
        void OnPlayerTriggerPressurePlate(TriggerPressurePlateEventArgs<Player> args)
        {
            Tile underBlock = Main.tile[args.TileX, args.TileY + 1];
            if (underBlock.type != JBID)
                return;
            if (!pressureTriggerEnable)
                return;
            else if (!TShock.Players[args.Object.whoAmI].HasPermission("terrajump.usepad"))
            {
                TShock.Players[args.Object.whoAmI].SendErrorMessage("You don't have permission to do this!");
                return;
            }
            else if (userlist.Contains(TShock.Players[args.Object.whoAmI].Name))
                return;
            else if (isDisabling.Contains(TShock.Players[args.Object.whoAmI]) && isDisable(args))
            {
                JPEnaNext(args.TileX, args.TileY, TShock.Players[args.Object.whoAmI]);
                Thread.Sleep(500);
            }
            else if (isDisable(args))
                return;

            TShock.Log.Info("Pressure plate triggered");
            //TShock.Log.ConsoleInfo("[PlTPP]Starting procedure");
            bool pds = false;
            TSPlayer ow = TShock.Players[args.Object.whoAmI];
            Tile pressurePlate = Main.tile[args.TileX, args.TileY];
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
            else
            {
                //TShock.Log.ConsoleInfo("[PlTPP]Can't find any SlimeBlocks! Stoping");
                return;
            }
            if (isDisabling.Contains(TShock.Players[args.Object.whoAmI]))
                JPDisNext(args.TileX, args.TileY, TShock.Players[args.Object.whoAmI]);
            //TShock.Log.Info("Found you!");
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
            getver =  readfile.ReadLine();
            readfile.Close();

            char stabileOrUnstabile = getver[6];

            int firstGetver = (int)getver[0];
            int secondGetver = (int)getver[2];
            int thirdGetver = (int)getver[4];

            int firstver = (int)ver[0];
            int secondver = (int)ver[2];
            int thirdver = (int)ver[4];

            /*TShock.Log.ConsoleInfo("Fisrt get " + firstGetver + " Second get " + secondGetver + " Third get " + thirdGetver);        // FOR DEVS ONLY!!
            TShock.Log.ConsoleInfo("Fisrt ver " + firstver + " Second ver " + secondver + " Third ver " + thirdver);*/

            if (firstGetver > firstver && secondGetver > secondver && thirdGetver > thirdver)
            {
                if(stabileOrUnstabile.ToString() == "U")
                {
                    TShock.Log.ConsoleInfo("[TerraJump Update] There is a new version! New version " + getver + "!");
                    TShock.Log.ConsoleInfo("[TerraJump Update] This new version is for unstable version!");
                    isUpdates = true;
                    return;
                }
                TShock.Log.ConsoleInfo("[TerraJump Update] There is a new version! New version " + getver + "!");
                isUpdates = true;
                return;
            }

            File.Delete(TShock.SavePath + @"\Ver.txt");

            isUpdates = false;
            TShock.Log.Info("No Updates");
            return;
        }
        //End do Check Update VOID



        //Is Disable
        bool isDisable(TriggerPressurePlateEventArgs<Player> args)
        {
            List<int> Xes = new List<int>();
            List<int> Yes = new List<int>();
            bool isDis = false;
            /*XYSet.Tables["XYJumpPads"].AsEnumerable().ForEach(xyy =>
            {
                if ((x.Equals((float)xyy["X"])) && (y.Equals((float)xyy["Y"])))
                {
                    isDis = true;
                    TShock.Log.Info("Found You!");
                }
            });*/

            foreach(DataRow dr in XYSet.Tables["XYJumpPads"].Rows)
            {
                Xes.Add(Convert.ToInt32(dr["X"]));
                Yes.Add(Convert.ToInt32(dr["Y"]));
            }

            if(Xes.Contains(args.TileX) && Yes.Contains(args.TileY))
            {
                //TShock.Log.Info("Found You!!");
                return true;
            }

            if (isDis)
                return true;
            else if (!isDis)
                return false;
            return false;
        }
        //End of Is disable



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