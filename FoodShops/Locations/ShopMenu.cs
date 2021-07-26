using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FoodShops.Locations
{
    /// <summary>
    /// The Menu items available to purchase on a shop.
    /// </summary>
    public class ShopMenu
    {
        /// <summary>
        /// The identifier of this menu.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public Guid ID { get; set; }
        /// <summary>
        /// The Meals that are part of this menu.
        /// </summary>
        [JsonProperty("meals", Required = Required.Always)]
        public List<Meal> Meals { get; set; }
    }
}
