using GTA;
using GTA.Math;
using GTA.Native;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using GTA.UI;
using PlayerCompanion;

namespace FoodShops
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
        public Shop Shop { get; }
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

        public Menu(Shop shop, ScaledTexture banner) : base("", shop.Name, "", banner)
        {
            Shop = shop;
            //NoItemsText = "This Shop does not has any food or drinks to be purchased.";
            foreach (ShopMenu menu in shop.Menus)
            {
                foreach (Meal meal in menu.Meals)
                {
                    NativeItem item = new NativeItem($"{meal.Name} ({meal.Health})", meal.Description ?? "", $"${meal.Price}")
                    {
                        Tag = meal
                    };
                    Add(item);
                }
            }

            Opening += PurchaseMenu_Opening;
            Shown += PurchaseMenu_Shown;
            ItemActivated += PurchaseMenu_ItemActivated;
            Closing += PurchaseMenu_Closing;
            Closed += PurchaseMenu_Closed;
        }

        #endregion
        
        #region Tools
        
        private static void PlayAnimationAndWait(string animDict, string animName, AnimationFlags animFlags)
        {
            Ped ped = Game.Player.Character;

            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, animDict))
            {
                Script.Yield();
            }

            Game.Player.Character.Task.PlayAnimation(animDict, animName, 8f, -8f, -1, animFlags, 0f);

            while (Function.Call<float>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, ped, animDict, animName) < 0.9f)
            {
                Script.Yield();
            }
        }
        
        #endregion

        #region Events

        private void PurchaseMenu_Opening(object sender, CancelEventArgs e)
        {
            Game.Player.CanControlCharacter = false;
            Game.Player.Character.Opacity = 0;
            
            Shop.Camera = World.CreateCamera(Shop.CameraInfo.Position, Vector3.Zero, Shop.CameraInfo.FOV);
            Function.Call(Hash.POINT_CAM_AT_PED_BONE, Shop.Camera, Shop.Ped, (int)Bone.SkelHead, 0, 0, 5, true);
            Shop.Ped.Task.LookAt(Shop.CameraInfo.Position);
            World.RenderingCamera = Shop.Camera;
        }
        private void PurchaseMenu_Shown(object sender, EventArgs e)
        {
            Shop.Ped.PlayAmbientSpeech("GENERIC_HI");
            MealsEaten = 0;
        }
        private void PurchaseMenu_ItemActivated(object sender, ItemActivatedArgs e)
        {
            if (!(e.Item.Tag is Meal meal))
            {
                return;
            }
            
            if (Companion.Wallet.Money < meal.Price)
            {
                Shop.Ped.PlayAmbientSpeech("GENERIC_CURSE_MED");
                Notification.Show("You don't have enough money to buy this!");
                return;
            }
            
            Companion.Wallet.Money -= meal.Price;
            float health = Game.Player.Character.HealthFloat + meal.Health;
            float maxHealth = Game.Player.Character.MaxHealthFloat == 0 ? Game.Player.Character.MaxHealth : Game.Player.Character.MaxHealthFloat;
            if (health > maxHealth)
            {
                health = maxHealth;
            }
            Game.Player.Character.HealthFloat = health;
            MealsEaten += 1;

            if (FoodShops.Config.MaxMeals > 1 && MealsEaten > FoodShops.Config.MaxMeals)
            {
                switch (FoodShops.Config.OverEatingBehavior)
                {
                    case OverEatingBehavior.Animation:
                        Shop.Ped.PlayAmbientSpeech("GENERIC_SHOCKED_MED");
                        PlayAnimationAndWait("missfam5_blackout", "vomit", AnimationFlags.None);
                        break;
                    case OverEatingBehavior.Death:
                        Game.Player.Character.Kill();
                        break;
                }
                
                Game.Player.Character.HealthFloat = HealthOnOpened;
            }
            else
            {
                Shop.Ped.PlayAmbientSpeech("GENERIC_THANKS");
            }
        }
        private void PurchaseMenu_Closing(object sender, CancelEventArgs e)
        {
            Vector3 pos = Game.Player.Character.Position;
            Game.Player.Character.PositionNoOffset = new Vector3(Shop.Trigger.X, Shop.Trigger.Y, pos.Z);
            Game.Player.CanControlCharacter = true;
        }
        private void PurchaseMenu_Closed(object sender, EventArgs e)
        {
            if (Shop.Camera != null)
            {
                Shop.Camera.Delete();
                Shop.Camera = null;
                World.RenderingCamera = null;
            }

            if (Shop.Ped.IsAlive && Game.Player.WantedLevel > 0)
            {
                Shop.Ped.PlayAmbientSpeech("GENERIC_SHOCKED_MED");
            }
            
            HealthOnOpened = -1;
            Game.Player.Character.Opacity = 255;
            Shop.Ped?.Task.ClearLookAt();
        }

        #endregion
    }
}
