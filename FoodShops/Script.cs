using FoodShops.Converters;
using GTA;
using GTA.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        #region Constructor

        public FoodShops()
        {
            // If the folder with meals exist, load them
            List<ShopMenu> menus = new List<ShopMenu>();
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

            // Then, do the same but for the shop locations themselves
            List<ShopLocation> locations = new List<ShopLocation>();
            string locationsPath = Path.Combine(location, "Locations");
            if (Directory.Exists(locationsPath))
            {
                ShopMenuConverter converter = new ShopMenuConverter(menus);
                foreach (string file in Directory.EnumerateFiles(menuPath))
                {
                    if (Path.GetExtension(file).ToLowerInvariant() != ".json")
                    {
                        Notification.Show($"~o~Warning~s~: Non JSON file found in the Locations Directory! ({Path.GetFileName(file)})");
                        continue;
                    }
                    string contents = File.ReadAllText(file);
                    ShopLocation location = JsonConvert.DeserializeObject<ShopLocation>(contents, converter);
                    locations.Add(location);
                }
            }
        }

        #endregion
    }
}
