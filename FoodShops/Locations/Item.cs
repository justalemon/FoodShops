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

        /// <summary>
        /// The meal handled by this item.
        /// </summary>
        public Meal Meal { get; }

        #endregion

        #region Constructor

        public Item(Meal meal) : base($"{meal.Name} ({meal.Health})", meal.Description ?? "", $"${meal.Price}")
        {
            Meal = meal;
        }

        #endregion
    }
}
