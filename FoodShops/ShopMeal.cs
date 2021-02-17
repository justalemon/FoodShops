using Newtonsoft.Json;

namespace FoodShops
{
    /// <summary>
    /// A Meal that can be purchased in a shop.
    /// </summary>
    public class ShopMeal
    {
        /// <summary>
        /// The name of the Meal.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// A short description of the Meal.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }
        /// <summary>
        /// The Price of the Meal.
        /// </summary>
        [JsonProperty("price")]
        public int Price { get; set; }
        /// <summary>
        /// The Level of Health that this Meal gives.
        /// </summary>
        [JsonProperty("health")]
        public float Health { get; set; }
    }
}
