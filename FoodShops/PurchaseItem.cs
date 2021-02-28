using GTA;
using GTA.Native;
using GTA.UI;
using LemonUI.Menus;
using PlayerCompanion;
using System;

namespace FoodShops
{
    /// <summary>
    /// The item used to Purchase specific meals.
    /// </summary>
    public class PurchaseItem : NativeItem
    {
        #region Fields

        private readonly ShopMeal meal;

        #endregion

        #region Constructor

        public PurchaseItem(ShopMeal meal) : base($"{meal.Name} ({meal.Health})", meal.Description ?? "", $"${meal.Price}")
        {
            this.meal = meal;
            Activated += PurchaseItem_Activated;
        }

        #endregion

        #region Events

        private void PurchaseItem_Activated(object sender, EventArgs e)
        {
            // If the sender is not a Purchase Menu
            if (!(sender is PurchaseMenu menu))
            {
                return;
            }

            // if the player does not has enough money to buy this meal, notify it and return
            if (Companion.Wallet.Money < meal.Price)
            {
                menu.Location.Ped.PlayAmbientSpeech("GENERIC_CURSE_MED");

                Notification.Show("You don't have enough money to buy this!");
                return;
            }

            // If the player has reached the meal limit, make him puke
            if (menu.MealsEaten >= 5)
            {
                menu.Location.Ped.PlayAmbientSpeech("GENERIC_SHOCKED_MED");

                Game.Player.Character.HealthFloat = menu.HealthOnOpened;
                menu.Close();
                Game.Player.CanControlCharacter = false;

                Function.Call(Hash.REQUEST_ANIM_DICT, "missfam5_blackout");
                while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, "missfam5_blackout"))
                {
                    Script.Yield();
                }
                Game.Player.Character.Task.PlayAnimation("missfam5_blackout", "vomit");
                while (Function.Call<float>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, Game.Player.Character, "missfam5_blackout", "vomit") < 0.99f)
                {
                    Script.Yield();
                }
                Game.Player.CanControlCharacter = true;
                return;
            }

            // Otherwise, remove the money and heal the player (if possible)
            menu.Location.Ped.PlayAmbientSpeech("GENERIC_THANKS");

            Companion.Wallet.Money -= meal.Price;
            float health = Game.Player.Character.HealthFloat + meal.Health;
            if (health <= Game.Player.Character.MaxHealthFloat)
            {
                Game.Player.Character.HealthFloat = health;
            }
            menu.MealsEaten += 1;
        }

        #endregion
    }
}
