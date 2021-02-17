using FoodShops.Converters;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

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

        private readonly List<ShopMenu> menus = new List<ShopMenu>();
        private readonly List<ShopLocation> locations = new List<ShopLocation>();

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
                        locations.Add(location);
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
        }

        #endregion

        #region Events

        private void FoodShops_Tick(object sender, EventArgs e)
        {
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
            }
        }

        #endregion
    }
}
