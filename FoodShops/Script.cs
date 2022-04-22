using FoodShops.Locations;
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

        private static readonly string dataDirectory = Path.Combine(new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath, "FoodShops");
        private static readonly List<Location> locations = new List<Location>();
        private static readonly Dictionary<Guid, ShopMenu> menus = new Dictionary<Guid, ShopMenu>();
        
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
        /// The currently active location.
        /// </summary>
        public static Location Active { get; private set; }

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

            Tick += FoodShops_Tick_Init;
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
        private static void PopulateLocations()
        {
            locations.Clear();

            ShopMenuConverter converter = new ShopMenuConverter(menus);

            foreach (string path in Directory.EnumerateFiles(Path.Combine(dataDirectory, "Locations")))
            {
                if (Path.GetExtension(path).ToLowerInvariant() != ".json")
                {
                    Notification.Show($"~o~Warning~s~: Non JSON file found in the Locations Directory! ({Path.GetFileName(path)})");
                    return;
                }

                try
                {
                    Location location = Location.Load(path, converter);
                    locations.Add(location);
                }
                catch (InteriorNotFoundException ex)
                {
                    Notification.Show($"~o~Warning~s~: Interior of {ex.Location.Name} is not available! Maybe you forgot to install it?");
                }
                catch (InvalidPedException ex)
                {
                    Notification.Show($"~o~Warning~s~: Model {ex.Location.PedInfo.Model} for Location {ex.Location.Name} is not a Ped!");
                }
                catch (Exception ex)
                {
                    Notification.Show($"~o~Warning~s~: Unable to load Location {Path.GetFileName(path)}:\n{ex.Message}");
                }
            }
        }

        #endregion

        #region Events

        private void FoodShops_Tick_Init(object sender, EventArgs e)
        {
            PopulateMenus();
            PopulateLocations();
            Tick -= FoodShops_Tick_Init;
            Tick += FoodShops_Tick_Run;
        }
        private void FoodShops_Tick_Run(object sender, EventArgs e)
        {
            Pool.Process();

            if (Active != null)
            {
                if (Pool.AreAnyVisible && (Active.Ped.IsFleeing || Active.Ped.IsDead || Game.Player.WantedLevel > 0))
                {
                    Pool.HideAll();
                    return;
                }
            }

            Vector3 pos = Game.Player.Character.Position;

            foreach (Location location in locations)
            {
                if (pos.DistanceTo(location.Trigger) > 50)
                {
                    if (location.Ped.IsFleeing || location.Ped.IsDead)
                    {
                        location.RecreatePed();
                    }
                    continue;
                }

                if (location.Ped.IsPositionFrozen && Function.Call<bool>(Hash.HAS_COLLISION_LOADED_AROUND_ENTITY, location.Ped))
                {
                    location.Ped.IsPositionFrozen = false;
                }
                
                if (location.Ped.IsPositionFrozen || location.Ped.IsFleeing || location.Ped.IsDead)
                {
                    continue;
                }
                
                World.DrawMarker(MarkerType.VerticalCylinder, location.Trigger, Vector3.Zero, Vector3.Zero, new Vector3(0.5f, 0.5f, 0.5f), Color.Red);

                if (pos.DistanceTo(location.Trigger) > 1.25f)
                {
                    continue;
                }
                
                Screen.ShowHelpTextThisFrame("Press ~INPUT_CONTEXT~ to buy some food.");
                
                if (Game.IsControlJustPressed(Control.Context))
                {
                    location.Menu.Visible = true;
                    Active = location;
                    return;
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

            foreach (Location location in locations)
            {
                location.Ped?.Delete();
                location.Blip?.Delete();
                location.Camera?.Delete();
            }
        }

        #endregion
    }
}
