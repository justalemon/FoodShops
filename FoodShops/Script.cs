using FoodShops.Locations;
using GTA;
using LemonUI;
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

        #endregion

        #region Constructor

        public FoodShops()
        {
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
