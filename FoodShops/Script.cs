using FoodShops.Converters;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using LemonUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using Screen = GTA.UI.Screen;

namespace FoodShops
{
    /// <summary>
    /// Main script for the Mod.
    /// </summary>
    public class FoodShops : Script
    {
        #region Fields

        internal static string location = Path.Combine(new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath, "FoodShops");

        #endregion

        #region Properties

        private readonly ObjectPool pool = new ObjectPool();
        private readonly List<ShopMenu> menus = new List<ShopMenu>();
        private readonly List<ShopLocation> locations = new List<ShopLocation>();
        private readonly Dictionary<ShopLocation, PurchaseMenu> uiMenus = new Dictionary<ShopLocation, PurchaseMenu>();

        #endregion

        #region Constructor

        public FoodShops()
        {
            // If the folder with meals exist, load them
            string menuPath = Path.Combine(location, "Menus");
            if (Directory.Exists(menuPath))
            {
                foreach (string file in Directory.EnumerateFiles(menuPath))
                {
                    if (Path.GetExtension(file).ToLowerInvariant() != ".json")
                    {
                        Notification.Show($"~o~Warning~s~: Non JSON file found in the Menus Directory! ({Path.GetFileName(file)})");
                        continue;
                    }
                    string contents = File.ReadAllText(file);
                    ShopMenu menu = JsonConvert.DeserializeObject<ShopMenu>(contents);
                    menus.Add(menu);
                }
            }
            else
            {
                Notification.Show($"~o~Warning~s~: Menus Directory was not found!");
            }

            // Then, do the same but for the shop locations themselves
            string locationsPath = Path.Combine(location, "Locations");
            if (Directory.Exists(locationsPath))
            {
                ShopMenuConverter converter = new ShopMenuConverter(menus);
                foreach (string file in Directory.EnumerateFiles(locationsPath))
                {
                    if (Path.GetExtension(file).ToLowerInvariant() != ".json")
                    {
                        Notification.Show($"~o~Warning~s~: Non JSON file found in the Locations Directory! ({Path.GetFileName(file)})");
                        continue;
                    }
                    string contents = File.ReadAllText(file);
                    ShopLocation location = JsonConvert.DeserializeObject<ShopLocation>(contents, converter);
                    if (Function.Call<bool>(Hash.IS_VALID_INTERIOR, location.Interior))
                    {
                        PurchaseMenu menu = new PurchaseMenu(location);
                        locations.Add(location);
                        pool.Add(menu);
                        uiMenus.Add(location, menu);
                    }
                    else
                    {
                        Notification.Show($"~o~Warning~s~: Skipping {Path.GetFileName(file)} because the Interior ID is not valid");
                    }
                }
            }
            else
            {
                Notification.Show($"~o~Warning~s~: Locations Directory was not found!");
            }

            // Finally, add the tick event and start working
            Tick += FoodShops_Tick;
            Aborted += FoodShops_Aborted;
        }

        #endregion

        #region Tools

        private void DisableControls()
        {
            Game.DisableControlThisFrame(Control.LookUpDown);
            Game.DisableControlThisFrame(Control.LookLeftRight);

            Game.DisableControlThisFrame(Control.MoveLeftRight);
            Game.DisableControlThisFrame(Control.MoveUpDown);
            Game.DisableControlThisFrame(Control.MoveUpOnly);
            Game.DisableControlThisFrame(Control.MoveDownOnly);
            Game.DisableControlThisFrame(Control.MoveLeftOnly);
            Game.DisableControlThisFrame(Control.MoveRightOnly);
            Game.DisableControlThisFrame(Control.MoveUp);
            Game.DisableControlThisFrame(Control.MoveDown);
            Game.DisableControlThisFrame(Control.MoveLeft);
            Game.DisableControlThisFrame(Control.MoveRight);
        }

        #endregion

        #region Events

        private void FoodShops_Tick(object sender, EventArgs e)
        {
            // Process the contents of the menus and return if anything is open
            if (pool.AreAnyVisible)
            {
                DisableControls();
            }
            pool.Process();
            if (pool.AreAnyVisible)
            {
                DisableControls();
                return;
            }

            // Get some of the user's information to use it later
            Vector3 pos = Game.Player.Character.Position;
            int interior = Function.Call<int>(Hash.GET_INTERIOR_FROM_ENTITY, Game.Player.Character);

            // Iterate over the available interiors
            foreach (ShopLocation location in locations)
            {
                // If the player is too far from the location or is not on the correct interior, skip it
                if (pos.DistanceTo(location.Trigger) > 50 || interior != location.Interior)
                {
                    continue;
                }

                // Otherwise, draw the marker in the correct position
                World.DrawMarker(MarkerType.VerticalCylinder, location.Trigger, Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), Color.Red);

                // If the player is very close to the ma1rker, tell him to press interact
                if (pos.DistanceTo(location.Trigger) < 1.25f)
                {
                    Screen.ShowHelpTextThisFrame("Press ~INPUT_CONTEXT~ to buy some food.");

                    if (Game.IsControlJustPressed(Control.Context))
                    {
                        uiMenus[location].Visible = true;
                    }
                }
            }
        }

        private void FoodShops_Aborted(object sender, EventArgs e)
        {
            pool.HideAll();
        }

        #endregion
    }
}
