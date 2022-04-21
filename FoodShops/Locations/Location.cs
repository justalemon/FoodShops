using FoodShops.Converters;
using GTA;
using GTA.Math;
using GTA.Native;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using LemonUI.Elements;

namespace FoodShops.Locations
{
    /// <summary>
    /// Represents the location of a specific shop.
    /// </summary>
    public class Location
    {
        #region Properties
        
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
        
        #endregion
        
        #region Functions

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

            if (Ped != null)
            {
                Ped.Delete();
            }

            Ped = World.CreatePed(PedInfo.Model, PedInfo.Position, PedInfo.Heading);
            PedInfo.Model.MarkAsNoLongerNeeded();
        }
        /// <summary>
        /// Loads a location.
        /// </summary>
        /// <param name="path">The file to load.</param>
        /// <param name="converters">The converters to use.</param>
        /// <returns>The location loaded</returns>
        /// <exception cref="InteriorNotFoundException">The interior required does not exists.</exception>
        /// <exception cref="InvalidPedException">The shop keeper is not a valid ped.</exception>
        public static Location Load(string path, params JsonConverter[] converters)
        {
            string contents = File.ReadAllText(path);
            Location location = JsonConvert.DeserializeObject<Location>(contents, converters);

            if (location.Interior.HasValue)
            {
                if (Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, location.Interior.Value.X, location.Interior.Value.Y, location.Interior.Value.Z) == 0)
                {
                    throw new InteriorNotFoundException(location);
                }
            }

            if (!location.PedInfo.Model.IsPed)
            {
                throw new InvalidPedException(location);
            }

            ScaledTexture texture = null;
            if (!string.IsNullOrWhiteSpace(location.BannerTXD) && !string.IsNullOrWhiteSpace(location.BannerTexture))
            {
                texture = new ScaledTexture(PointF.Empty, new SizeF(0, 108), location.BannerTXD, location.BannerTexture);
            }
            Menu menu = new Menu(location, texture);
            FoodShops.Pool.Add(menu);
            location.Menu = menu;

            location.RecreatePed();

            if (FoodShops.Config.ShowBlips)
            {
                location.Blip = World.CreateBlip(location.Trigger);
                location.Blip.Sprite = BlipSprite.Store;
                location.Blip.Color = BlipColor.NetPlayer3;
                location.Blip.Name = $"Food Shop: {location.Name}";
                location.Blip.IsShortRange = true;
            }

            return location;
        }
        
        #endregion
    }
}
