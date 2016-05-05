using System;
using System.Linq;

using Ensage;
using SharpDX;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common;
using SharpDX.Direct3D9;
using System.Windows.Input;

namespace BrewMaster_ControlCreep
{
    internal class Program
    {

        private static bool activated;
        private static Font txt;
        private static Font not;
		private static Hero me;
		private static Vector3 tpos;
		private static Ability blink, ulti;
        private static Key KeyControl = Key.L;
		private static Key KeyOff = Key.P;
		private static readonly Menu Menu = new Menu("ControlCreep", "ControlCreep", true, "npc_dota_hero_brewmaster", true);



        static void Main(string[] args)
        {
			Game.OnUpdate += Game_Sincan2;
            Game.OnWndProc += Game_OnWndProc;
            Console.WriteLine("> ControlCreep By Sincan2 loaded!");
			Menu.AddItem(new MenuItem("Deactivation Key", "Activation Key").SetValue(new KeyBind('P', KeyBindType.Press)));
			Menu.AddItem(new MenuItem("Activation Key", "Deactivation Key").SetValue(new KeyBind('L', KeyBindType.Press)));
			Menu.AddItem(new MenuItem("Primalstorm: Use Cyclone", "Primalstorm: Use Cyclone").SetValue(false).SetTooltip("If disabled, casts Cyclone only in targets, which channeling abilitys like: tp's, blackhole's, deathward's and e.t.c."));
			Menu.AddItem(new MenuItem("Primalstorm: Use Dispel Magic", "Primalstorm: Use Dispel Magic").SetValue(false).SetTooltip("If enabled, always safe mana for Cyclon."));
			Menu.AddItem(new MenuItem("Save mana for Cyclone", "Save mana for Cyclone").SetValue(false).SetTooltip("Do not cast Dispel Magic, Drunken Haze or Invisability if after cast there will be no mana for Cyclone."));
            Menu.AddToMainMenu();

            txt = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Calibri",
                   Height = 26,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });

            not = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Calibri",
                   Height = 22,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        }




        public static void Game_Sincan2(EventArgs args)
        {	var me = ObjectMgr.LocalHero;
			if (!Game.IsInGame || me == null)
			{
				return;
			}
		
            me = ObjectMgr.LocalHero;
            var target = me.ClosestToMouseTarget(1200);
			var targets = ObjectMgr.GetEntities<Hero>().Where(enemy => enemy.Team == me.GetEnemyTeam() && !enemy.IsIllusion() && enemy.IsVisible && enemy.IsAlive && enemy.Health > 0).ToList();


            if (target.IsAlive && !target.IsInvul() && (Game.MousePosition.Distance2D(target) <= 1000 || target.Distance2D(me) <= 600))
            {
			var CheckDrunken = target.Modifiers.Any(y => y.Name == "modifier_brewmaster_drunken_haze");
			var CheckClap = target.Modifiers.Any(y => y.Name == "modifier_brewmaster_thunder_clap");              
				//Illusions	
                var illus = ObjectMgr.GetEntities<Hero>().Where(x => x.IsAlive && x.IsControllable && x.Team == me.Team && x.IsIllusion).ToList();
				if (illus == null)
                {
                    return;
                }
				if (activated)
				{
	            foreach (Unit illusion in illus.TakeWhile(illusion => Utils.SleepCheck("illu")))
                        {
                            illusion.Attack(target);
                            Utils.Sleep(1000, "illu");
                        }
				}
				
                var primalearth = ObjectMgr.GetEntities<Unit>().Where(x => (x.ClassID == ClassID.CDOTA_Unit_Brewmaster_PrimalEarth)
                    && x.IsAlive && x.IsControllable);
                if (primalearth == null)
                {
                    return;
                }
                    foreach (var v in primalearth)
                    {

                        if (target.Position.Distance2D(v.Position) < 850 && v.Spellbook.SpellQ.CanBeCasted() && activated &&
                            Utils.SleepCheck(v.Handle.ToString()))
                        {
                            v.Spellbook.SpellQ.UseAbility(target);
                            Utils.Sleep(400, v.Handle.ToString());
                        }
                        if (target.Position.Distance2D(v.Position) < 300 && v.Spellbook.SpellR.CanBeCasted() && activated && !CheckClap &&
                           Utils.SleepCheck(v.Handle.ToString()))
                        {
                            v.Spellbook.SpellR.UseAbility();
                            Utils.Sleep(400, v.Handle.ToString());
                        }

                        if (target.Position.Distance2D(v.Position) < 1550 && activated &&
                            Utils.SleepCheck(v.Handle.ToString()))
                        {
                            v.Attack(target);
                            Utils.Sleep(700, v.Handle.ToString());
                        }
                    }
                
                var primalstorm = ObjectMgr.GetEntities<Unit>().Where(x => (x.ClassID == ClassID.CDOTA_Unit_Brewmaster_PrimalStorm)
				
                       && x.IsAlive && x.IsControllable);
                if (primalstorm == null)
                {
                    return;
                }
               

                    foreach (var v in primalstorm)
                    {
					if (target.Position.Distance2D(v.Position) < 500 && v.Spellbook.SpellQ.CanBeCasted() && (Menu.Item("Primalstorm: Use Dispel Magic").GetValue<bool>()) && (!(Menu.Item("Save mana for Cyclone").GetValue<bool>()) || (v.Mana > (v.Spellbook.SpellQ.ManaCost + v.Spellbook.SpellW.ManaCost))) &&
                            Utils.SleepCheck(v.Handle.ToString()))
                        {
                            v.Spellbook.SpellQ.UseAbility(target.Position);
                            Utils.Sleep(400, v.Handle.ToString());
                        }
					if (target.Position.Distance2D(v.Position) < 900 && v.Spellbook.SpellE.CanBeCasted() && activated && (!(Menu.Item("Save mana for Cyclone").GetValue<bool>()) || (v.Mana > (v.Spellbook.SpellE.ManaCost + v.Spellbook.SpellW.ManaCost))) &&
						Utils.SleepCheck(v.Handle.ToString()))
					{
						v.Spellbook.SpellE.UseAbility();
						Utils.Sleep(400, v.Handle.ToString());
					}
					if (target.Position.Distance2D(v.Position) < 850 && v.Spellbook.SpellR.CanBeCasted() && activated && !CheckDrunken && (!(Menu.Item("Save mana for Cyclone").GetValue<bool>()) || (v.Mana > (v.Spellbook.SpellR.ManaCost + v.Spellbook.SpellW.ManaCost))) &&
                           Utils.SleepCheck(v.Handle.ToString()))
                        {
                            v.Spellbook.SpellR.UseAbility(target);
                            Utils.Sleep(400, v.Handle.ToString());
                        }
                        if (target.Position.Distance2D(v.Position) < 1550 && activated &&
                           Utils.SleepCheck(v.Handle.ToString()))
                        {
                            v.Attack(target);
                            Utils.Sleep(700, v.Handle.ToString());
                        }
                    }
				// 2 Skill
		
				foreach (var target1 in targets)
                {
                    if ((target1.Health > (target1.MaximumHealth*0.85)) || ((target1.IsChanneling())) )
                    { 
						foreach (var v in primalstorm)
                    {
						ulti = v.Spellbook.SpellW;
						if (v.Spellbook.SpellW.CanBeCasted() && ((target1.Position.Distance2D(v.Position) < 600) || (target1.IsChanneling())) && (target1.Position.Distance2D(v.Position) < 1550) && ((Menu.Item("Primalstorm: Use Cyclone").GetValue<bool>()) || (target1.IsChanneling())) && ((target1.Position != target.Position) || (target1.IsChanneling())) &&
                           Utils.SleepCheck("ulti")) 
						{
						v.Spellbook.SpellW.UseAbility(target1);
                        Utils.Sleep(700, "ulti");	
						}
					}
                    }
                }
				//
                

                var primalfire = ObjectMgr.GetEntities<Unit>().Where(x => (x.ClassID == ClassID.CDOTA_Unit_Brewmaster_PrimalFire)
                       && x.IsAlive && x.IsControllable);
                if (primalfire == null)
                {
                    return;
                }
                    foreach (var v in primalfire)
                    {

                        if (target.Position.Distance2D(v.Position) < 1550 && activated &&
                            Utils.SleepCheck(v.Handle.ToString()))
                        {
                            v.Attack(target);
                            Utils.Sleep(700, v.Handle.ToString());
                        }
                    }





                
			}
		}



        private static void Game_OnWndProc(EventArgs args)
        {
            if (Game.IsKeyDown(Menu.Item("Activation Key").GetValue<KeyBind>().Key) && (activated = false) && !Game.IsChatOpen && Utils.SleepCheck("toggle"))
            {
                activated = true;
                Utils.Sleep(250, "toggle");
            }
			if (Game.IsKeyDown(Menu.Item("Deactivation Key").GetValue<KeyBind>().Key) && (activated = true) && !Game.IsChatOpen && Utils.SleepCheck("toggle"))
            {
                activated = false;
                Utils.Sleep(250, "toggle");
            }
        }

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
				txt.DrawText(null, "Control is  ON", 908, 881, Color.Black);
                txt.DrawText(null, "Control is  ON", 907, 880, Color.Lime);
            }

            if (!activated)
            {
				txt.DrawText(null, "Control is OFF", 908, 881, Color.Black);
                txt.DrawText(null, "Control is OFF", 907, 880, Color.Red);
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
    }
}
