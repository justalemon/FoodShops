using FoodShops.Converters;
using GTA;
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
        /// The dictionary where the banner texture is.
        /// </summary>
        [JsonProperty("banner_txd")]
        public string BannerTXD { get; set; }
        /// <summary>
        /// The texture to use as a banner.
        /// </summary>
        [JsonProperty("banner_texture")]
        public string BannerTexture { get; set; }
        /// <summary>
        /// The marker trigger used to open the menu.
        /// </summary>
        [JsonProperty("trigger")]
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Trigger { get; set; }
        /// <summary>
        /// The position of the ped over the counter.
        /// </summary>
        [JsonProperty("ped_pos")]
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 PedPos { get; set; }
        /// <summary>
        /// The Heading of the ped.
        /// </summary>
        [JsonProperty("ped_heading")]
        public float PedHeading { get; set; }
        /// <summary>
        /// The model of the ped.
        /// </summary>
        [JsonProperty("ped_model")]
        public Model PedModel { get; set; }
        /// <summary>
        /// The position of the camera.
        /// </summary>
        [JsonProperty("cam_pos")]
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 CamPos { get; set; }
        /// <summary>
        /// The Field of View of the camera.
        /// </summary>
        [JsonProperty("cam_fov")]
        public float CamFOV { get; set; }
        /// <summary>
        /// The Meal Menus that the player can consume.
        /// </summary>
        [JsonProperty("menus")]
        public List<ShopMenu> Menus { get; set; }

        /// <summary>
        /// The Ped over the counter that speaks with the player.
        /// </summary>
        [JsonIgnore]
        public Ped Ped { get; private set; }

        /// <summary>
        /// Creates the ped over the counter.
        /// </summary>
        public void CreatePed()
        {
            DeletePed();
            Ped = World.CreatePed(PedModel, PedPos, PedHeading);
            Ped.BlockPermanentEvents = true;
            Ped.CanBeTargetted = false;
            Ped.CanRagdoll = false;
            Ped.CanWrithe = false;
            Ped.IsInvincible = true;
        }
        /// <summary>
        /// Deletes the existing ped, if any.
        /// </summary>
        public void DeletePed()
        {
            if (Ped != null)
            {
                Ped.Delete();
            }
            Ped = null;
        }
    }
}
