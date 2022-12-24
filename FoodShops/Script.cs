using GTA;
using GTA.UI;
using LemonUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using FoodShops.Converters;
using GTA.Math;
using GTA.Native;
using Screen = GTA.UI.Screen;

namespace FoodShops
{
    /// <summary>
    /// Main script for the Mod.
    /// </summary>
    public class FoodShops : Script
    {
        #region Fields

        private static readonly TimeSpan hoursSix = TimeSpan.FromHours(6);
        private static readonly TimeSpan hoursTwelve = TimeSpan.FromHours(6);
        private static readonly TimeSpan hoursTwentyFour = TimeSpan.FromHours(24);

        private static readonly string dataDirectory = Path.Combine(new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath, "FoodShops");
        private static readonly List<Shop> shops = new List<Shop>();
        private static readonly Dictionary<Guid, ShopMenu> menus = new Dictionary<Guid, ShopMenu>();
        private static DateTime drunkUntil;

        #endregion
        
        #region Properties

        /// <summary>
        /// The LemonUI object pool of this mod.
        /// </summary>
        public static ObjectPool Pool { get; } = new ObjectPool();
        /// <summary>
        /// The main configuration of the script.
        /// </summary>
        public static Configuration Config { get; private set; } = new Configuration();
        /// <summary>
        /// The currently active shop.
        /// </summary>
        public static Shop Active { get; private set; }
        /// <summary>
        /// If the player just became drunk.
        /// </summary>
        public static bool JustBecameDrunk { get; internal set; }

        /// <summary>
        /// Until when the player will be drunk, in in-game time.
        /// </summary>
        public static DateTime DrunkUntil
        {
            get => drunkUntil;
            internal set
            {
                if (drunkUntil == default)
                {
                    JustBecameDrunk = true;
                }
                
                drunkUntil = value;
            }
        }

        #endregion

        #region Constructor

        public FoodShops()
        {
            string path = Path.Combine(dataDirectory, "Config.json");
            try
            {
                Config = Configuration.Load(path);
            }
            catch (JsonException ex)
            {
                Notification.Show($"~o~Warning~s~: Unable to load the configuration file:\n{ex.GetType()}: {ex}");
                throw;
            }
            catch (SystemException ex)
            {
                Notification.Show($"~o~Warning~s~: Unable to save the configuration file:\n{ex.GetType()}: {ex}");
                Config = new Configuration();
            }

            Tick += FoodShops_Init;
            Aborted += FoodShops_Aborted;
        }

        #endregion
        
        #region Tools

        private static void PopulateMenus()
        {
            menus.Clear();

            string directory = Path.Combine(dataDirectory, "Menus");

            foreach (string path in Directory.EnumerateFiles(directory))
            {
                if (Path.GetExtension(path).ToLowerInvariant() != ".json")
                {
                    Notification.Show($"~o~Warning~s~: Non JSON file found in the Menus Directory! ({Path.GetFileName(path)})");
                    continue;
                }

                try
                {
                    string contents = File.ReadAllText(path);
                    ShopMenu menu = JsonConvert.DeserializeObject<ShopMenu>(contents);
                    menus.Add(menu.ID, menu);
                }
                catch (JsonException ex)
                {
                    Notification.Show($"~o~Warning~s~: Unable to load Menu {Path.GetFileName(path)}:\n{ex.Message}");
                }
            }
        }
        private static void PopulateShops()
        {
            shops.Clear();

            ShopMenuConverter converter = new ShopMenuConverter(menus);

            foreach (string path in Directory.EnumerateFiles(Path.Combine(dataDirectory, "Locations")))
            {
                if (Path.GetExtension(path).ToLowerInvariant() != ".json")
                {
                    Notification.Show($"~o~Warning~s~: Non JSON file found in the Shops Directory! ({Path.GetFileName(path)})");
                    return;
                }

                Shop shop = Shop.Load(path, converter);
                if (shop != null)
                {
                    shops.Add(shop);
                }
            }
        }
        private static void ClearDrunkenness()
        {
            Function.Call(Hash._SET_FACIAL_CLIPSET_OVERRIDE, Game.Player.Character, "mood_happy_1");
            Function.Call(Hash.CLEAR_FACIAL_IDLE_ANIM_OVERRIDE, Game.Player.Character);
            Function.Call((Hash)0x487A82C650EB7799, 0f);  // SET_GAMEPLAY_CAM_MOTION_BLUR_SCALING_THIS_UPDATE
            Function.Call((Hash)0x0225778816FDC28C, 0f);  // SET_GAMEPLAY_CAM_MAX_MOTION_BLUR_STRENGTH_THIS_UPDATE
            Function.Call(Hash.SET_GAMEPLAY_CAM_SHAKE_AMPLITUDE, 0f);
            Function.Call(Hash.STOP_SCRIPT_GLOBAL_SHAKING, false);
            Function.Call(Hash.SET_PED_IS_DRUNK, Game.Player.Character, false);
            Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, Game.Player.Character, 1f);
            Function.Call((Hash)0x1CBA05AE7BD7EE05, 15f);  // SET_TRANSITION_OUT_OF_TIMECYCLE_MODIFIER
            DrunkUntil = default;
        }

        #endregion

        #region Events

        private void FoodShops_Init(object sender, EventArgs e)
        {
            PopulateMenus();
            PopulateShops();
            Tick -= FoodShops_Init;
            Tick += FoodShops_Tick;
        }
        private void FoodShops_Tick(object sender, EventArgs e)
        {
            Pool.Process();

            if (DrunkUntil != default)
            {
                if (JustBecameDrunk)
                {
                    Function.Call(Hash._SET_FACIAL_CLIPSET_OVERRIDE, Game.Player.Character, "mood_drunk_1");
                    Function.Call(Hash.SET_FACIAL_IDLE_ANIM_OVERRIDE, Game.Player.Character, "mood_drunk_1", "facials@gen_male@base");
                    Function.Call((Hash)0x487A82C650EB7799, 1f);  // SET_GAMEPLAY_CAM_MOTION_BLUR_SCALING_THIS_UPDATE
                    Function.Call((Hash)0x0225778816FDC28C, 1f);  // SET_GAMEPLAY_CAM_MAX_MOTION_BLUR_STRENGTH_THIS_UPDATE
                    Function.Call(Hash.SHAKE_GAMEPLAY_CAM, "DRUNK_SHAKE", 1f);
                    Function.Call(Hash.SET_PED_IS_DRUNK, Game.Player.Character, true);
                    if (!Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, "move_m@drunk@verydrunk"))
                    {
                        Function.Call(Hash.REQUEST_ANIM_SET, "move_m@drunk@verydrunk");
                        Yield();
                    }
                    Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, Game.Player.Character, "move_m@drunk@verydrunk", 0x3E800000);
                    Function.Call(Hash.SET_TRANSITION_TIMECYCLE_MODIFIER, "Drunk", 15f);

                    JustBecameDrunk = false;
                }

                TimeSpan difference = DrunkUntil - World.CurrentDate;

                if (difference > hoursTwentyFour)
                {
                    Active.Menu.Visible = false;
                    Game.Player.Character.Health = 0;
                    ClearDrunkenness();
                    return;
                }

                if (DrunkUntil < World.CurrentDate)
                {
                    ClearDrunkenness();
                }

                if (difference < hoursSix)
                {
                    Function.Call(Hash.SET_AUDIO_SPECIAL_EFFECT_MODE, 5);  // kSpecialEffectModeDrunkStage01
                    Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, Game.Player.Character, 1f);
                }
                else if (difference < hoursTwelve)
                {
                    Function.Call(Hash.SET_AUDIO_SPECIAL_EFFECT_MODE, 6);  // kSpecialEffectModeDrunkStage02
                }
                else
                {
                    Function.Call(Hash.SET_AUDIO_SPECIAL_EFFECT_MODE, 7);  // kSpecialEffectModeDrunkStage03
                }
            }

            if (Active != null)
            {
                if (Pool.AreAnyVisible && (Active.Ped.IsFleeing || Active.Ped.IsDead || Game.Player.WantedLevel > 0))
                {
                    Pool.HideAll();
                    return;
                }
            }

            Vector3 pos = Game.Player.Character.Position;

            foreach (Shop shop in shops)
            {
                if (pos.DistanceTo(shop.Trigger) > 50)
                {
                    if (shop.Ped.IsFleeing || shop.Ped.IsDead)
                    {
                        shop.RecreatePed();
                    }
                    continue;
                }

                bool isAreaLoaded = Function.Call<bool>(Hash.HAS_COLLISION_LOADED_AROUND_ENTITY, shop.Ped);

                if (shop.Interior.HasValue)
                {
                    int id = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, shop.Interior.Value.X, shop.Interior.Value.Y, shop.Interior.Value.Z);
                    isAreaLoaded = isAreaLoaded && Function.Call<bool>(Hash.IS_INTERIOR_READY, id);
                }

                if (shop.Ped.IsPositionFrozen)
                {
                    if (isAreaLoaded)
                    {
                        shop.Ped.IsPositionFrozen = false;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (!isAreaLoaded)
                    {
                        shop.Ped.IsPositionFrozen = true;
                        shop.Ped.Position = shop.PedInfo.Position;
                        shop.Ped.Heading = shop.PedInfo.Heading;
                        continue;
                    }
                }

                if (shop.Ped.IsPositionFrozen || shop.Ped.IsFleeing || shop.Ped.IsDead)
                {
                    continue;
                }

                World.DrawMarker(MarkerType.VerticalCylinder, shop.Trigger, Vector3.Zero, Vector3.Zero, new Vector3(0.5f, 0.5f, 0.5f), Color.Red);

                if (pos.DistanceTo(shop.Trigger) > 1.25f)
                {
                    continue;
                }

                Screen.ShowHelpTextThisFrame("Press ~INPUT_CONTEXT~ to buy some food.");
                
                if (Game.IsControlJustPressed(Control.Context))
                {
                    shop.Menu.Visible = true;
                    Active = shop;
                    break;
                }
            }
        }

        private void FoodShops_Aborted(object sender, EventArgs e)
        {
            if (Active != null)
            {
                Game.Player.CanControlCharacter = true;
                Game.Player.Character.Opacity = 255;
                World.RenderingCamera = null;
            }

            foreach (Shop shop in shops)
            {
                shop.Ped?.Delete();
                shop.Blip?.Delete();
                shop.Camera?.Delete();
            }
            
            ClearDrunkenness();
        }

        #endregion
    }
}
