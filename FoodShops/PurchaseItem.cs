using GTA;
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
            // if the player does not has enough money to buy this meal, notify it and return
            if (Companion.Wallet.Money < meal.Price)
            {
                Notification.Show("You don't have enough money to buy this!");
                return;
            }

            // Otherwise, remove the money and heal the player (if possible)
            Companion.Wallet.Money -= meal.Price;
            float health = Game.Player.Character.HealthFloat + meal.Health;
            if (health < Game.Player.Character.MaxHealthFloat)
            {
                Game.Player.Character.HealthFloat = health;
            }
        }

        #endregion
    }
}
