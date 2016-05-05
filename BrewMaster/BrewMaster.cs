using System;
using System.Collections.Generic;

using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common;
using SharpDX;
using Ensage.Common.Menu;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D9;
using System.Globalization;
using System.Windows.Input;

namespace BrewMaster
{
    class BrewMaster
    {
        private static bool activated;
		private static readonly Menu Menu = new Menu("BrewMaster", "BrewMaster", true, "npc_dota_hero_brewmaster", true);
        private static readonly Menu Menu_Items = new Menu("Items: ", "Items: ");
		private static readonly Menu Menu_Skills = new Menu("Skills: ", "Skills: ");
        private static Ability thunderclap, drunkenhaze, primalsplit;
        private static Item blink, bkb, orchid, dust, urn, medal, shiva, manta, bloodthorn, heavens_halberd;
        private static ParticleEffect particleEffect;
        private static Hero me, target;
        private static Vector3 mousepos;
		private static Font txt;
        private static Font not;
		private static Key KeyControl = Key.M;
        private static readonly Dictionary<string, bool> items = new Dictionary<string, bool>
            {
                {"item_blink",true},
                {"item_black_king_bar",true},
                {"item_orchid",true},
				{"item_heavens_halberd",true},
				{"item_bloodthorn",true},
				{"item_dust",true},
				{"item_urn_of_shadows",true},
				{"item_medallion_of_courage",true},
				{"item_shivas_guard",true},
				{"item_manta",true},
            };
        private static readonly Dictionary<string, bool> Skills = new Dictionary<string, bool>
            {
                {"brewmaster_thunder_clap",true},
                {"brewmaster_drunken_haze",true},
				{"brewmaster_primal_split",true},
            };
        static void Main(string[] args)
        {
            Menu.AddItem(new MenuItem("Combo Key", "Combo Key").SetValue(new KeyBind('D', KeyBindType.Press)));
			Menu.AddItem(new MenuItem("Using Ultimate", "Using Ultimate").SetValue(new KeyBind('M', KeyBindType.Press)));
            Menu.AddSubMenu(Menu_Items);
            Menu_Items.AddItem(new MenuItem("Items: ", "Items: ").SetValue(new AbilityToggler(items)));
			Menu.AddSubMenu(Menu_Skills);
            Menu_Skills.AddItem(new MenuItem("Skills: ", "Skills: ").SetValue(new AbilityToggler(Skills)));
            Menu.AddToMainMenu();
            PrintSuccess("> BrewMaster Script!");
            Game.OnUpdate += Universe;
			//
			txt = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Calibri",
                   Height = 46,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });

            not = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Calibri",
                   Height = 46,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        }
        public static void Universe(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Brewmaster)
                return;
			FindItems();
			//manta (when silenced)
			if ((manta != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(manta.Name)) && manta.CanBeCasted() && me.IsSilenced() && Utils.SleepCheck("manta"))
             {
                manta.UseAbility();
                Utils.Sleep(400 + Game.Ping, "manta");
             }
			if (Game.IsKeyDown(Menu.Item("Using Ultimate").GetValue<KeyBind>().Key) && !Game.IsChatOpen && Utils.SleepCheck("toggle"))
            {
                activated = !activated;
                Utils.Sleep(250, "toggle");
            }
            if (Game.IsKeyDown(Menu.Item("Combo Key").GetValue<KeyBind>().Key) && !Game.IsChatOpen)
            {
                if (me.CanCast())
                {
                    mousepos = Game.MousePosition;
					var target = me.ClosestToMouseTarget(1200);
					var CheckDrunken = target.Modifiers.Any(y => y.Name == "modifier_brewmaster_drunken_haze");
                    if (me.Distance2D(mousepos) <= 1200)
                    {
                            //drunken haze (main combo)
							if ((drunkenhaze != null && Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(drunkenhaze.Name)) && drunkenhaze.CanBeCasted() && ((target.Position.Distance2D(me.Position) < 850) && (target.Position.Distance2D(me.Position) > 300)) && primalsplit.CanBeCasted() && Utils.SleepCheck("thunderclap"))
                            {
                                drunkenhaze.UseAbility(target);
                                Utils.Sleep(150 + Game.Ping, "drunkenhaze");
                            }
							//drunken haze (if can't cast ult) --->Сюда добавить переменную отвечающую за ручное выключение ульты из комбо && если ульт выключен
							if ((drunkenhaze != null && Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(drunkenhaze.Name)) && drunkenhaze.CanBeCasted() && target.Position.Distance2D(me.Position) < 850 && (!primalsplit.CanBeCasted() || (target.Health < (target.MaximumHealth*0.50)) || !activated || !(Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(primalsplit.Name))) && !CheckDrunken && (target.Health > (target.MaximumHealth*0.07)) && Utils.SleepCheck("drunkenhaze"))
                            {
                                drunkenhaze.UseAbility(target);
                                Utils.Sleep(150 + Game.Ping, "drunkenhaze");
                            }
							//black king bar
							if ((bkb != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(bkb.Name)) && bkb.CanBeCasted() && Utils.SleepCheck("bkb"))
                            {
                                bkb.UseAbility();
                                Utils.Sleep(150 + Game.Ping, "bkb");
                            }
							//blink
                            if ((blink != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(blink.Name)) && blink.CanBeCasted() && target.Position.Distance2D(me.Position) > 300 && Utils.SleepCheck("blink"))
                            {
                                blink.UseAbility(me.Distance2D(mousepos) < 1200 ? mousepos : new Vector3(me.NetworkPosition.X + 1150 * (float)Math.Cos(me.NetworkPosition.ToVector2().FindAngleBetween(mousepos.ToVector2(), true)), me.NetworkPosition.Y + 1150 * (float)Math.Sin(me.NetworkPosition.ToVector2().FindAngleBetween(mousepos.ToVector2(), true)), 100), false);
                                Utils.Sleep(150 + Game.Ping, "blink");
                            }
							//orchid
							if ((orchid != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(orchid.Name)) && orchid.CanBeCasted() && (target.Position.Distance2D(me.Position) < 300) && Utils.SleepCheck("orchid"))
                            {
                                orchid.UseAbility(target);
                                Utils.Sleep(150 + Game.Ping, "orchid");
                            }
							//heavens_halberd
							if ((heavens_halberd != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(heavens_halberd.Name)) && heavens_halberd.CanBeCasted() && (target.Position.Distance2D(me.Position) < 300) && Utils.SleepCheck("heavens_halberd"))
                            {
                                heavens_halberd.UseAbility(target);
                                Utils.Sleep(150 + Game.Ping, "heavens_halberd");
                            }
							//bloodthorn
							if ((bloodthorn != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(bloodthorn.Name)) && bloodthorn.CanBeCasted() && (target.Position.Distance2D(me.Position) < 300) && Utils.SleepCheck("bloodthorn"))
                            {
                                bloodthorn.UseAbility(target);
                                Utils.Sleep(150 + Game.Ping, "bloodthorn");
                            }
							//dust
							if ((dust != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(dust.Name)) && dust.CanBeCasted() && Utils.SleepCheck("dust"))
                            {
                                dust.UseAbility();
                                Utils.Sleep(150 + Game.Ping, "dust");
                            }
							//thunder clap
                            if ((thunderclap != null && Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(thunderclap.Name)) && thunderclap.CanBeCasted() && (target.Position.Distance2D(me.Position) < 300) && Utils.SleepCheck("thunderclap"))
                            {
                                thunderclap.UseAbility();
                                Utils.Sleep(150 + Game.Ping, "thunderclap");
                            }
							//urn of shadow
							if ((urn != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(urn.Name)) && urn.CanBeCasted() && Utils.SleepCheck("urn"))
                            {
                                urn.UseAbility(target);
                                Utils.Sleep(150 + Game.Ping, "urn");
                            }
							//medal
							if ((medal != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(medal.Name)) && medal.CanBeCasted() && (target.Position.Distance2D(me.Position) < 300) && Utils.SleepCheck("medal"))
                            {
                                medal.UseAbility(target);
                                Utils.Sleep(150 + Game.Ping, "medal");
                            }
							//shiva
							if ((shiva != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(shiva.Name)) && shiva.CanBeCasted() && (target.Position.Distance2D(me.Position) <= 800) && Utils.SleepCheck("shiva"))
                            {
                                shiva.UseAbility();
                                Utils.Sleep(150 + Game.Ping, "shiva");
                            }
							//manta
							if ((manta != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(manta.Name)) && manta.CanBeCasted() && (target.Position.Distance2D(me.Position) <= 450) && Utils.SleepCheck("manta"))
                            {
                                manta.UseAbility();
                                Utils.Sleep(150 + Game.Ping, "manta");
                            }
							//ULTIMATE: PrimalSplit
                            if ((primalsplit != null && Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(primalsplit.Name)) && primalsplit.CanBeCasted() && (target.Position.Distance2D(me.Position) < 500) && (target.Health > (target.MaximumHealth*0.35)) && !thunderclap.CanBeCasted() && !orchid.CanBeCasted() && !heavens_halberd.CanBeCasted() && !bloodthorn.CanBeCasted() && !dust.CanBeCasted() && !urn.CanBeCasted() && !medal.CanBeCasted() && !shiva.CanBeCasted() && !manta.CanBeCasted() && activated && Utils.SleepCheck("primalsplit"))
                            {
                                primalsplit.UseAbility();
                                Utils.Sleep(1000 + Game.Ping, "primalsplit");
								var primalstorm = ObjectMgr.GetEntities<Unit>().FirstOrDefault(unit => unit.ClassID == ClassID.CDOTA_Unit_Brewmaster_PrimalStorm && unit.IsAlive);
			
                
                            }
							//Moving+Attaking
							if (me.CanMove() && (target.Position.Distance2D(me.Position) > 200))
                            {
                                me.Move(mousepos, false);
                            }
						    if (me.CanMove() && (target.Position.Distance2D(me.Position) < 200))
                            {
                                me.Attack(target);
                            }
                    }
                    else
                    {
                        if (me.CanMove())
                        {
                            me.Move(mousepos, false);
                        }
                    }
                }
            }
	
        }
        static void FindItems()
        {
            if (Utils.SleepCheck("FINDITEMS"))
            {
                blink = me.FindItem("item_blink");
                bkb = me.FindItem("item_black_king_bar");
				orchid = me.FindItem("item_orchid");
				heavens_halberd = me.FindItem("item_heavens_halberd");
				bloodthorn = me.FindItem("item_bloodthorn");
				dust = me.FindItem("item_dust");
				urn = me.FindItem("item_urn_of_shadows");
				medal = me.FindItem("item_medallion_of_courage");
				shiva = me.FindItem("item_shivas_guard");
				manta = me.FindItem("item_manta");
                thunderclap = me.Spellbook.SpellQ;
                drunkenhaze = me.Spellbook.SpellW;
				primalsplit = me.Spellbook.SpellR;
                Utils.Sleep(500, "FINDITEMS");
            }
        }
		

		
		// Ultimate Using
		
		static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            txt.Dispose();
            not.Dispose();
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
                return;

            var player = ObjectMgr.LocalPlayer;


            if (activated)
            {
				txt.DrawText(null, "ON", 1256, 981, Color.Black);
                txt.DrawText(null, "ON", 1255, 980, Color.Lime);
            }

            if (!activated)
            {
				txt.DrawText(null, "OFF", 1256, 981, Color.Black);
                txt.DrawText(null, "OFF", 1255, 980, Color.Red);
            }
        }



		static void Drawing_OnPostReset(EventArgs args)
        {
            txt.OnResetDevice();
            not.OnResetDevice();
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
            txt.OnLostDevice();
            not.OnLostDevice();
        }
        //
        private static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }
        private static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }
    }
}
