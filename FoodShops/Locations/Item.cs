using GTA;
using GTA.UI;
using LemonUI.Menus;
using PlayerCompanion;
using System;

namespace FoodShops.Locations
{
    /// <summary>
    /// The item used to Purchase specific meals.
    /// </summary>
    public class Item : NativeItem
    {
        #region Fields

        private readonly Meal meal;

        #endregion

        #region Constructor

        public Item(Meal meal) : base($"{meal.Name} ({meal.Health})", meal.Description ?? "", $"${meal.Price}")
        {
            this.meal = meal;
            Activated += PurchaseItem_Activated;
        }

        #endregion

        #region Events

        private void PurchaseItem_Activated(object sender, EventArgs e)
        {
            // If the sender is not a Purchase Menu
            if (!(sender is Menu menu))
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

            // If the player has reached the meal limit, return and let the manager play the puke animation
            if (menu.MealsEaten > 5)
            {
                menu.MealsEaten += 1;
                Game.Player.Character.HealthFloat = menu.HealthOnOpened;
                return;
            }

            // Otherwise, remove the money and heal the player (if possible)
            menu.Location.Ped.PlayAmbientSpeech("GENERIC_THANKS");

            Companion.Wallet.Money -= meal.Price;
            float health = Game.Player.Character.HealthFloat + meal.Health;
            float maxHealth = Game.Player.Character.MaxHealthFloat;
            if (health > maxHealth)
            {
                health = maxHealth;
            }
            Game.Player.Character.HealthFloat = health;
            menu.MealsEaten += 1;
        }

        #endregion
    }
}
