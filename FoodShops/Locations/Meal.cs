using Newtonsoft.Json;
using System.ComponentModel;

namespace FoodShops.Locations
{
    /// <summary>
    /// A Meal that can be purchased in a shop.
    /// </summary>
    public class Meal
    {
        /// <summary>
        /// The name of the Meal.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }
        /// <summary>
        /// A short description of the Meal.
        /// </summary>
        [JsonProperty("description", Required = Required.DisallowNull)]
        [DefaultValue("")]
        public string Description { get; set; }
        /// <summary>
        /// The Price of the Meal.
        /// </summary>
        [JsonProperty("price", Required = Required.Always)]
        public int Price { get; set; }
        /// <summary>
        /// The Level of Health that this Meal gives.
        /// </summary>
        [JsonProperty("health", Required = Required.Always)]
        public float Health { get; set; }
    }
}
