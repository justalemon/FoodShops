﻿using FoodShops.Converters;
using GTA;
using GTA.Math;
using GTA.Native;
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
        public ShopPed PedInfo { get; set; }
        /// <summary>
        /// The information of the shop camera.
        /// </summary>
        [JsonProperty("camera", Required = Required.Always)]
        public ShopCamera CameraInfo { get; set; }
        /// <summary>
        /// The Meal Menus that the player can consume.
        /// </summary>
        [JsonProperty("menus", Required = Required.Always)]
        public List<ShopMenu> Menus { get; set; }

        /// <summary>
        /// The camera used when the menu is open.
        /// </summary>
        [JsonIgnore]
        public Camera Camera { get; private set; }
        /// <summary>
        /// The Blip used to mark the location of the Food Shop.
        /// </summary>
        [JsonIgnore]
        public Blip Blip { get; private set; }
        /// <summary>
        /// The Ped over the counter that speaks with the player.
        /// </summary>
        [JsonIgnore]
        public Ped Ped { get; private set; }

        /// <summary>
        /// Initializes the Entities and Camera.
        /// </summary>
        public void Initialize()
        {
            // Request the ped and create it
            PedInfo.Model.Request();
            while (!PedInfo.Model.IsLoaded)
            {
                Script.Yield();
            }
            Ped = World.CreatePed(PedInfo.Model, PedInfo.Position, PedInfo.Heading);
            Ped.IsPositionFrozen = true;
            Ped.BlockPermanentEvents = true;
            Ped.CanBeTargetted = false;
            Ped.CanRagdoll = false;
            Ped.CanWrithe = false;
            Ped.IsInvincible = true;
            PedInfo.Model.MarkAsNoLongerNeeded();
            // Then, create the blip of the Food Shop
            Blip = World.CreateBlip(Trigger);
            Blip.Sprite = BlipSprite.Store;
            Blip.Color = BlipColor.NetPlayer3;
            Blip.Name = $"Food Shop: {Name}";
            Blip.IsShortRange = true;
            // Finally, create the camera
            Camera = World.CreateCamera(CameraInfo.Position, Vector3.Zero, CameraInfo.FOV);
            Function.Call(Hash.POINT_CAM_AT_PED_BONE, Camera, Ped, (int)Bone.SkelHead, 0, 0, 5, true);
            Ped.Task.LookAt(CameraInfo.Position);
        }
        /// <summary>
        /// Cleans up the entities and camera.
        /// </summary>
        public void DoCleanup()
        {
            Ped?.Delete();
            Blip?.Delete();
            Camera?.Delete();
        }
    }
}
