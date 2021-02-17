using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FoodShops
{
    /// <summary>
    /// The Menu items available to purchase on a shop.
    /// </summary>
    public class ShopMenu
    {
        /// <summary>
        /// The identifier of this menu.
        /// </summary>
        [JsonProperty("id")]
        public Guid ID { get; set; }
        /// <summary>
        /// The Meals that are part of this menu.
        /// </summary>
        [JsonProperty("meals")]
        public List<ShopMeal> Meals { get; set; }
    }
}
