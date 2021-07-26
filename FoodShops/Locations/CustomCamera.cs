using FoodShops.Converters;
using GTA.Math;
using Newtonsoft.Json;

namespace FoodShops.Locations
{
    /// <summary>
    /// Represents the location of a camera.
    /// </summary>
    public class CustomCamera
    {
        /// <summary>
        /// The position of the camera.
        /// </summary>
        [JsonProperty("pos", Required = Required.Always)]
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Position { get; set; }
        /// <summary>
        /// The Field of View of the camera.
        /// </summary>
        [JsonProperty("fov", Required = Required.Always)]
        public float FOV { get; set; }
    }
}
