using FoodShops.Locations;
using GTA;
using GTA.UI;
using LemonUI;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace FoodShops
{
    /// <summary>
    /// Main script for the Mod.
    /// </summary>
    public class FoodShops : Script
    {
        #region Properties

        /// <summary>
        /// The directory where the mod data is stored.
        /// </summary>
        public static string DataDirectory { get; } = Path.Combine(new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath, "FoodShops");
        /// <summary>
        /// The LemonUI object pool of this mod.
        /// </summary>
        public static ObjectPool Pool { get; } = new ObjectPool();
        /// <summary>
        /// The main configuration of the script.
        /// </summary>
        public static Configuration Config { get; private set; } = new Configuration();

        #endregion

        #region Constructor

        public FoodShops()
        {
            string path = Path.Combine(DataDirectory, "Config.json");
            try
            {
                Config = Configuration.Load(path);
            }
            catch (JsonException ex)
            {
                Notification.Show($"~o~Warning~s~: Unable to load the configuration file:\n{ex.GetType().FullName}: {ex}");
                throw;
            }
            catch (SystemException ex)
            {
                Notification.Show($"~o~Warning~s~: Unable to save the configuration file:\n{ex.GetType().FullName}: {ex}");
                Config = new Configuration();
            }

            Tick += FoodShops_Tick_Init;
            Aborted += FoodShops_Aborted;
        }

        #endregion

        #region Events

        private void FoodShops_Tick_Init(object sender, EventArgs e)
        {
            LocationManager.Populate();
            Tick -= FoodShops_Tick_Init;
            Tick += FoodShops_Tick_Run;
        }
        private void FoodShops_Tick_Run(object sender, EventArgs e)
        {
            Pool.Process();
            LocationManager.Process();
        }

        private void FoodShops_Aborted(object sender, EventArgs e)
        {
            LocationManager.DoCleanup();
        }

        #endregion
    }
}
