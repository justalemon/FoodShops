using FoodShops.Converters;
using GTA.Math;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FoodShops
{
    /// <summary>
    /// Represents the location of a specific shop.
    /// </summary>
    public class ShopLocation
    {
        /// <summary>
        /// The name of this Shop.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// The interior where this Shop is located.
        /// </summary>
        [JsonProperty("interior")]
        public int Interior { get; set; }
        /// <summary>
        /// The marker trigger used to open the menu.
        /// </summary>
        [JsonProperty("trigger")]
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Trigger { get; set; }
        /// <summary>
        /// The Meal Menus that the player can consume.
        /// </summary>
        [JsonProperty("menus")]
        public List<ShopMenu> Menus { get; set; }
    }
}
