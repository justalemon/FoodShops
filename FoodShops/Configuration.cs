using Newtonsoft.Json;

namespace FoodShops
{
    /// <summary>
    /// The general configuration of FoodShops.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// The maximum amount of meals before the user vomits.
        /// </summary>
        [JsonProperty("max_meals", Required = Required.Always)]
        public int MaxMeals { get; set; } = 5;
    }
}
