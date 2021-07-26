using FoodShops.Converters;
using GTA;
using GTA.Math;
using GTA.Native;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FoodShops.Locations
{
    /// <summary>
    /// Represents the location of a specific shop.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// The name of this Shop.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }
        /// <summary>
        /// The dictionary where the banner texture is.
        /// </summary>
        [JsonProperty("banner_txd", Required = Required.Always)]
        public string BannerTXD { get; set; }
        /// <summary>
        /// The texture to use as a banner.
        /// </summary>
        [JsonProperty("banner_texture", Required = Required.Always)]
        public string BannerTexture { get; set; }
        /// <summary>
        /// The marker trigger used to open the menu.
        /// </summary>
        [JsonProperty("trigger", Required = Required.Always)]
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Trigger { get; set; }
        /// <summary>
        /// The location where an interior should be checked.
        /// </summary>
        [JsonProperty("interior", Required = Required.AllowNull)]
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3? Interior { get; set; }
        /// <summary>
        /// The information of the shop ped.
        /// </summary>
        [JsonProperty("ped", Required = Required.Always)]
        public PedInfo PedInfo { get; set; }
        /// <summary>
        /// The information of the shop camera.
        /// </summary>
        [JsonProperty("camera", Required = Required.Always)]
        public CustomCamera CameraInfo { get; set; }
        /// <summary>
        /// The Meal Menus that the player can consume.
        /// </summary>
        [JsonProperty("menus", Required = Required.Always)]
        public List<ShopMenu> Menus { get; set; }

        /// <summary>
        /// The menu used at this location.
        /// </summary>
        public Menu Menu { get; internal set; }
        /// <summary>
        /// The camera used when the menu is open.
        /// </summary>
        [JsonIgnore]
        public Camera Camera { get; set; }
        /// <summary>
        /// The Blip used to mark the location of the Food Shop.
        /// </summary>
        [JsonIgnore]
        public Blip Blip { get; set; }
        /// <summary>
        /// The Ped over the counter that speaks with the player.
        /// </summary>
        [JsonIgnore]
        public Ped Ped { get; set; }

        /// <summary>
        /// Recreates the Shop Keeper.
        /// </summary>
        public void RecreatePed()
        {
            PedInfo.Model.Request();
            while (!PedInfo.Model.IsLoaded)
            {
                Script.Yield();
            }
            Ped = World.CreatePed(PedInfo.Model, PedInfo.Position, PedInfo.Heading);
            PedInfo.Model.MarkAsNoLongerNeeded();
        }
    }
}
