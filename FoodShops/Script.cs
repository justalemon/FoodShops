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
        #region Fields

        internal static string location = Path.Combine(new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath, "FoodShops");
        internal static readonly ObjectPool pool = new ObjectPool();

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
            pool.Process();
            LocationManager.Process();
        }

        private void FoodShops_Aborted(object sender, EventArgs e)
        {
            LocationManager.DoCleanup();
        }

        #endregion
    }
}
