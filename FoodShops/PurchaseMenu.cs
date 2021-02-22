using GTA;
using GTA.Math;
using GTA.Native;
using LemonUI.Elements;
using LemonUI.Menus;
using System;

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
            Closed += PurchaseMenu_Closed;
        }

        #endregion

        #region Events

        private void PurchaseMenu_Shown(object sender, EventArgs e)
        {
            Camera camera = World.CreateCamera(location.CamPos, Vector3.Zero, location.CamFOV);
            if (location.Ped == null)
            {
                location.CreatePed();
            }
            Function.Call(Hash.POINT_CAM_AT_PED_BONE, camera, location.Ped, (int)Bone.SkelHead, 0, 0, 5, true);
            Game.Player.Character.Opacity = 0;
            World.RenderingCamera = camera;
        }

        private void PurchaseMenu_Closed(object sender, EventArgs e)
        {
            Game.Player.Character.Opacity = 255;
            World.RenderingCamera = null;
        }

        #endregion
    }
}
