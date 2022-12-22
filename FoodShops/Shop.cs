using System;
using FoodShops.Converters;
using GTA;
using GTA.Math;
using GTA.Native;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using GTA.UI;
using LemonUI.Elements;

namespace FoodShops
{
    /// <summary>
    /// Represents the location of a specific shop.
    /// </summary>
    public class Shop
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
            Ped.IsPositionFrozen = true;
            PedInfo.Model.MarkAsNoLongerNeeded();
        }
        /// <summary>
        /// Loads a specific shop.
        /// </summary>
        /// <param name="path">The file to load.</param>
        /// <param name="converters">The converters to use.</param>
        /// <returns>The location loaded</returns>
        public static Shop Load(string path, params JsonConverter[] converters)
        {
            string contents = File.ReadAllText(path);

            try
            {
                Shop shop = JsonConvert.DeserializeObject<Shop>(contents, converters);

                if (shop.Interior.HasValue)
                {
                    int interior = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, shop.Interior.Value.X,
                        shop.Interior.Value.Y, shop.Interior.Value.Z);

                    if (interior == 0)
                    {
                        Notification.Show(
                            $"~o~Warning~s~: Interior of {shop.Name} is not available! Maybe you forgot to install it?");
                        return null;
                    }

                    Function.Call(Hash.PIN_INTERIOR_IN_MEMORY, interior);
                    Function.Call(Hash.DISABLE_INTERIOR, interior, false);
                    Function.Call(Hash.CAP_INTERIOR, interior, false);
                    Function.Call(Hash.SET_INTERIOR_ACTIVE, interior, true);
                }

                if (!shop.PedInfo.Model.IsPed)
                {
                    Notification.Show($"~o~Warning~s~: Model {shop.PedInfo.Model} for Shop {shop.Name} is not a Ped!");
                    return null;
                }

                ScaledTexture texture = null;
                if (!string.IsNullOrWhiteSpace(shop.BannerTXD) && !string.IsNullOrWhiteSpace(shop.BannerTexture))
                {
                    texture = new ScaledTexture(PointF.Empty, new SizeF(0, 108), shop.BannerTXD, shop.BannerTexture);
                }

                Menu menu = new Menu(shop, texture);
                FoodShops.Pool.Add(menu);
                shop.Menu = menu;

                shop.RecreatePed();

                if (FoodShops.Config.ShowBlips)
                {
                    shop.Blip = World.CreateBlip(shop.Trigger);
                    shop.Blip.Sprite = BlipSprite.Store;
                    shop.Blip.Color = BlipColor.NetPlayer3;
                    shop.Blip.Name = $"Food Shop: {shop.Name}";
                    shop.Blip.IsShortRange = true;
                }

                return shop;
            }
            catch (Exception ex)
            {
                Notification.Show($"~o~Warning~s~: Unable to load Shop {Path.GetFileName(path)}:\n{ex.Message}");
                return null;
            }
        }
        
        #endregion
    }
}
