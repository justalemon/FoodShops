using Newtonsoft.Json;

namespace FoodShops
{
    /// <summary>
    /// The behavior when the player over eats.
    /// </summary>
    public enum OverEatingBehavior
    {
        Animation = 0,
        Death = 1,
    }

    /// <summary>
    /// The general configuration of FoodShops.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// If the blips should be shown on the radar.
        /// </summary>
        [JsonProperty("show_blips")]
        public bool ShowBlips { get; set; } = true;
        /// <summary>
        /// The maximum amount of meals before the user vomits.
        /// </summary>
        [JsonProperty("max_meals")]
        public int MaxMeals { get; set; } = 5;
        /// <summary>
        /// What the mod should do when the player eats more food than allowed.
        /// </summary>
        [JsonProperty("over_eating_behavior")]
        public OverEatingBehavior OverEatingBehavior { get; set; } = OverEatingBehavior.Death;
    }
}
