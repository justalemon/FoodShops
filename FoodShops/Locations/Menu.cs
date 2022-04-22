using GTA;
using GTA.Math;
using GTA.Native;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using GTA.UI;
using PlayerCompanion;

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

            Opening += PurchaseMenu_Opening;
            Shown += PurchaseMenu_Shown;
            ItemActivated += PurchaseMenu_ItemActivated;
            Closing += PurchaseMenu_Closing;
            Closed += PurchaseMenu_Closed;
        }

        #endregion

        #region Events

        private void PurchaseMenu_Opening(object sender, CancelEventArgs e)
        {
            Game.Player.CanControlCharacter = false;
            Game.Player.Character.Opacity = 0;
            
            Location.Camera = World.CreateCamera(Location.CameraInfo.Position, Vector3.Zero, Location.CameraInfo.FOV);
            Function.Call(Hash.POINT_CAM_AT_PED_BONE, Location.Camera, Location.Ped, (int)Bone.SkelHead, 0, 0, 5, true);
            Location.Ped.Task.LookAt(Location.CameraInfo.Position);
            World.RenderingCamera = Location.Camera;
        }
        private void PurchaseMenu_Shown(object sender, EventArgs e)
        {
            Location.Ped.PlayAmbientSpeech("GENERIC_HI");
            MealsEaten = 0;
        }
        private void PurchaseMenu_ItemActivated(object sender, ItemActivatedArgs e)
        {
            if (!(e.Item is Item item))
            {
                return;
            }
            
            if (Companion.Wallet.Money < item.Meal.Price)
            {
                Location.Ped.PlayAmbientSpeech("GENERIC_CURSE_MED");
                Notification.Show("You don't have enough money to buy this!");
                return;
            }
            
            Companion.Wallet.Money -= item.Meal.Price;
            float health = Game.Player.Character.HealthFloat + item.Meal.Health;
            float maxHealth = Game.Player.Character.MaxHealthFloat;
            if (health > maxHealth)
            {
                health = maxHealth;
            }
            Game.Player.Character.HealthFloat = health;
            MealsEaten += 1;

            if (MealsEaten > FoodShops.Config.MaxMeals)
            {
                switch (FoodShops.Config.OverEatingBehavior)
                {
                    case OverEatingBehavior.Animation:
                        Location.Ped.PlayAmbientSpeech("GENERIC_SHOCKED_MED");
                        Tools.PlayAnimationAndWait("missfam5_blackout", "vomit", AnimationFlags.None);
                        break;
                    case OverEatingBehavior.Death:
                        Game.Player.Character.Kill();
                        break;
                }
                
                Game.Player.Character.HealthFloat = HealthOnOpened;
            }
            else
            {
                Location.Ped.PlayAmbientSpeech("GENERIC_THANKS");
            }
        }
        private void PurchaseMenu_Closing(object sender, CancelEventArgs e)
        {
            Vector3 pos = Game.Player.Character.Position;
            Game.Player.Character.PositionNoOffset = new Vector3(Location.Trigger.X, Location.Trigger.Y, pos.Z);
            Game.Player.CanControlCharacter = true;
        }
        private void PurchaseMenu_Closed(object sender, EventArgs e)
        {
            if (Location.Camera != null)
            {
                Location.Camera.Delete();
                Location.Camera = null;
                World.RenderingCamera = null;
            }

            if (Location.Ped.IsAlive && Game.Player.WantedLevel > 0)
            {
                Location.Ped.PlayAmbientSpeech("GENERIC_SHOCKED_MED");
            }
            
            HealthOnOpened = -1;
            Game.Player.Character.Opacity = 255;
            Location.Ped?.Task.ClearLookAt();
        }

        #endregion
    }
}
