﻿using FoodShops.Converters;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using LemonUI.Elements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace FoodShops.Locations
{
    /// <summary>
    /// Manages the different locations in the game world.
    /// </summary>
    public static class LocationManager
    {
        #region Fields

        private static readonly List<Location> locations = new List<Location>();
        private static readonly Dictionary<Guid, ShopMenu> menus = new Dictionary<Guid, ShopMenu>();

        #endregion

        #region Properties

        /// <summary>
        /// The currently active location.
        /// </summary>
        public static Location Active { get; private set; }

        #endregion

        #region Tool

        private static void PopulateSpecificMenu(string path)
        {
            if (Path.GetExtension(path).ToLowerInvariant() != ".json")
            {
                Notification.Show($"~o~Warning~s~: Non JSON file found in the Menus Directory! ({Path.GetFileName(path)})");
                return;
            }

            try
            {
                string contents = File.ReadAllText(path);
                ShopMenu @new = JsonConvert.DeserializeObject<ShopMenu>(contents);
                menus.Add(@new.ID, @new);
            }
            catch (Exception ex)
            {
                Notification.Show($"~o~Warning~s~: Unable to load Menu {Path.GetFileName(path)}:\n{ex.Message}");
            }
        }
        private static void PopulateSpecificLocation(string path, params JsonConverter[] converters)
        {
            if (Path.GetExtension(path).ToLowerInvariant() != ".json")
            {
                Notification.Show($"~o~Warning~s~: Non JSON file found in the Locations Directory! ({Path.GetFileName(path)})");
                return;
            }

            try
            {
                string contents = File.ReadAllText(path);
                Location location = JsonConvert.DeserializeObject<Location>(contents, converters);

                if (location.Interior.HasValue)
                {
                    if (Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, location.Interior.Value.X, location.Interior.Value.Y, location.Interior.Value.Z) == 0)
                    {
                        Notification.Show($"~o~Warning~s~: Interior of {location.Name} is not available! Maybe you forgot to install it?");
                        return;
                    }
                }

                if (!location.PedInfo.Model.IsPed)
                {
                    Notification.Show($"~o~Warning~s~: Model {location.PedInfo.Model} for Location {location.Name} is not a Ped!");
                    return;
                }

                ScaledTexture texture = null;
                if (!string.IsNullOrWhiteSpace(location.BannerTXD) && !string.IsNullOrWhiteSpace(location.BannerTexture))
                {
                    texture = new ScaledTexture(PointF.Empty, new SizeF(0, 108), location.BannerTXD, location.BannerTexture);
                }
                Menu menu = new Menu(location, texture);
                FoodShops.pool.Add(menu);
                location.Menu = menu;

                location.RecreatePed();

                location.Blip = World.CreateBlip(location.Trigger);
                location.Blip.Sprite = BlipSprite.Store;
                location.Blip.Color = BlipColor.NetPlayer3;
                location.Blip.Name = $"Food Shop: {location.Name}";
                location.Blip.IsShortRange = true;

                locations.Add(location);
            }
            catch (Exception ex)
            {
                Notification.Show($"~o~Warning~s~: Unable to load Location {Path.GetFileName(path)}:\n{ex.Message}");
            }
        }
        private static void PopulateMenus()
        {
            menus.Clear();

            string path = Path.Combine(FoodShops.location, "Menus");

            foreach (string file in Directory.EnumerateFiles(path))
            {
                PopulateSpecificMenu(file);
            }
        }
        private static void PopulateLocations()
        {
            locations.Clear();

            string path = Path.Combine(FoodShops.location, "Locations");
            ShopMenuConverter converter = new ShopMenuConverter(menus);

            foreach (string file in Directory.EnumerateFiles(path))
            {
                PopulateSpecificLocation(file, converter);
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Loads the different Menus and Locations.
        /// </summary>
        public static void Populate()
        {
            PopulateMenus();
            PopulateLocations();
        }
        /// <summary>
        /// Handles the current set of Locations.
        /// </summary>
        public static void Process()
        {
            if (Active != null)
            {
                if (!FoodShops.pool.AreAnyVisible || Active.Ped.IsFleeing || Active.Ped.IsDead || Game.Player.WantedLevel > 0)
                {
                    Active.Menu.Visible = false;
                    Active.Menu.MealsEaten = 0;

                    if (Active.Camera != null)
                    {
                        Active.Camera.Delete();
                        Active.Camera = null;
                        World.RenderingCamera = null;
                    }

                    if (Active.Menu.Items.Count > 0)
                    {
                        Active.Menu.SelectedIndex = 0;
                    }

                    Active = null;
                }
                return;
            }

            if (Game.Player.WantedLevel > 0)
            {
                return;
            }

            Vector3 pos = Game.Player.Character.Position;

            foreach (Location location in locations)
            {
                if (pos.DistanceTo(location.Trigger) > 50 || location.Ped.IsFleeing || location.Ped.IsDead)
                {
                    continue;
                }

                World.DrawMarker(MarkerType.VerticalCylinder, location.Trigger, Vector3.Zero, Vector3.Zero, new Vector3(0.5f, 0.5f, 0.5f), Color.Red);
                
                if (pos.DistanceTo(location.Trigger) < 1.25f)
                {
                    Screen.ShowHelpTextThisFrame("Press ~INPUT_CONTEXT~ to buy some food.");

                    if (Game.IsControlJustPressed(Control.Context))
                    {
                        location.Menu.Visible = true;

                        location.Camera = World.CreateCamera(location.CameraInfo.Position, Vector3.Zero, location.CameraInfo.FOV);
                        Function.Call(Hash.POINT_CAM_AT_PED_BONE, location.Camera, location.Ped, (int)Bone.SkelHead, 0, 0, 5, true);
                        location.Ped.Task.LookAt(location.CameraInfo.Position);
                        World.RenderingCamera = location.Camera;

                        Active = location;
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// Cleans up the Locations.
        /// </summary>
        public static void DoCleanup()
        {
            if (Active != null)
            {
                Game.Player.CanControlCharacter = true;
                Game.Player.Character.Opacity = 255;
                World.RenderingCamera = null;
            }

            foreach (Location location in locations)
            {
                location.Ped?.Delete();
                location.Blip?.Delete();
                location.Camera?.Delete();
            }
        }

        #endregion
    }
}