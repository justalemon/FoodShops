﻿using GTA;
using GTA.Math;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.ComponentModel;

namespace FoodShops
{
    /// <summary>
    /// The menu used to purchase things.
    /// </summary>
    public class PurchaseMenu : NativeMenu
    {
        #region Fields

        private readonly ShopLocation location;

        #endregion

        #region Constructor

        public PurchaseMenu(ShopLocation location, ScaledTexture banner) : base("", location.Name, "", banner)
        {
            this.location = location;
            //NoItemsText = "This Shop does not has any food or drinks to be purchased.";
            foreach (ShopMenu menu in location.Menus)
            {
                foreach (ShopMeal meal in menu.Meals)
                {
                    Add(new PurchaseItem(meal));
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
            Game.Player.Character.Opacity = 0;
            World.RenderingCamera = location.Camera;
        }

        private void PurchaseMenu_Closing(object sender, CancelEventArgs e)
        {
            Vector3 pos = Game.Player.Character.Position;
            Game.Player.Character.PositionNoOffset = new Vector3(location.Trigger.X, location.Trigger.Y, pos.Z);
        }

        private void PurchaseMenu_Closed(object sender, EventArgs e)
        {
            location.Ped?.Task.ClearLookAt();
            Game.Player.Character.Opacity = 255;
            World.RenderingCamera = null;
        }

        #endregion
    }
}
