using GTA;
using GTA.Math;
using GTA.Native;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.ComponentModel;

namespace FoodShops.Locations
{
    /// <summary>
    /// The menu used to purchase things.
    /// </summary>
    public class Menu : NativeMenu
    {
        #region Properties

        /// <summary>
        /// The location of the shop that this menu manages.
        /// </summary>
        public Location Location { get; }
        /// <summary>
        /// The number of meals that the player has eaten on this menu.
        /// </summary>
        public int MealsEaten { get; set; } = 0;
        /// <summary>
        /// The health that the player had when the menu was opened.
        /// </summary>
        public float HealthOnOpened { get; set; } = -1;

        #endregion

        #region Constructor

        public Menu(Location location, ScaledTexture banner) : base("", location.Name, "", banner)
        {
            Location = location;
            //NoItemsText = "This Shop does not has any food or drinks to be purchased.";
            foreach (ShopMenu menu in location.Menus)
            {
                foreach (Meal meal in menu.Meals)
                {
                    Add(new Item(meal));
                }
            }
            Shown += PurchaseMenu_Shown;
            Closing += PurchaseMenu_Closing;
            Closed += PurchaseMenu_Closed;
        }

        #endregion

        #region Events

        private void PurchaseMenu_Shown(object sender, EventArgs e)
        {
            Location.Ped.PlayAmbientSpeech("GENERIC_HI");
            MealsEaten = 0;
        }

        private void PurchaseMenu_Closing(object sender, CancelEventArgs e)
        {
            Vector3 pos = Game.Player.Character.Position;
            Game.Player.Character.PositionNoOffset = new Vector3(Location.Trigger.X, Location.Trigger.Y, pos.Z);
        }

        private void PurchaseMenu_Closed(object sender, EventArgs e)
        {
            HealthOnOpened = -1;
            Game.Player.Character.Opacity = 255;
            Location.Ped?.Task.ClearLookAt();
        }

        #endregion
    }
}
