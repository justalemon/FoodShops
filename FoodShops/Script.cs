﻿using FoodShops.Converters;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using LemonUI;
using LemonUI.Elements;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using Screen = GTA.UI.Screen;

namespace FoodShops
{
    /// <summary>
    /// Main script for the Mod.
    /// </summary>
    public class FoodShops : Script
    {
        #region Fields

        internal static string location = Path.Combine(new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath, "FoodShops");

        #endregion

        #region Properties

        private readonly ObjectPool pool = new ObjectPool();
        private readonly Dictionary<ShopLocation, PurchaseMenu> locations = new Dictionary<ShopLocation, PurchaseMenu>();

        #endregion

        #region Constructor

        public FoodShops()
        {
            // If the folder with meals exist, load them
            List<ShopMenu> menus = new List<ShopMenu>();
            string menuPath = Path.Combine(location, "Menus");
            if (Directory.Exists(menuPath))
            {
                foreach (string file in Directory.EnumerateFiles(menuPath))
                {
                    if (Path.GetExtension(file).ToLowerInvariant() != ".json")
                    {
                        Notification.Show($"~o~Warning~s~: Non JSON file found in the Menus Directory! ({Path.GetFileName(file)})");
                        continue;
                    }
                    string contents = File.ReadAllText(file);
                    ShopMenu menu = null;
                    try
                    {
                        menu = JsonConvert.DeserializeObject<ShopMenu>(contents);
                    }
                    catch (JsonSerializationException e)
                    {
                        Notification.Show($"~o~Warning~s~: Unable to load Menu {Path.GetFileName(file)}:\n{e.Message}");
                        continue;
                    }
                    menus.Add(menu);
                }
            }
            else
            {
                Notification.Show($"~o~Warning~s~: Menus Directory was not found!");
            }

            // Then, do the same but for the shop locations themselves
            string locationsPath = Path.Combine(location, "Locations");
            if (Directory.Exists(locationsPath))
            {
                ShopMenuConverter converter = new ShopMenuConverter(menus);
                foreach (string file in Directory.EnumerateFiles(locationsPath))
                {
                    if (Path.GetExtension(file).ToLowerInvariant() != ".json")
                    {
                        Notification.Show($"~o~Warning~s~: Non JSON file found in the Locations Directory! ({Path.GetFileName(file)})");
                        continue;
                    }

                    string contents = File.ReadAllText(file);
                    ShopLocation location = null;
                    try
                    {
                        location = JsonConvert.DeserializeObject<ShopLocation>(contents, converter);
                    }
                    catch (JsonSerializationException e)
                    {
                        Notification.Show($"~o~Warning~s~: Unable to load Location {Path.GetFileName(file)}:\n{e.Message}");
                        continue;
                    }

                    if (location.Interior.HasValue)
                    {
                        if (Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, location.Interior.Value.X, location.Interior.Value.Y, location.Interior.Value.Z) == 0)
                        {
                            Notification.Show($"~o~Warning~s~: Interior of {location.Name} is not available! Maybe you forgot to install it?");
                            continue;
                        }
                    }

                    if (!location.PedInfo.Model.IsPed)
                    {
                        Notification.Show($"~o~Warning~s~: Model {location.PedInfo.Model} is not a Ped!");
                        continue;
                    }

                    ScaledTexture texture = null;
                    if (!string.IsNullOrWhiteSpace(location.BannerTXD) && !string.IsNullOrWhiteSpace(location.BannerTexture))
                    {
                        texture = new ScaledTexture(PointF.Empty, new SizeF(0, 108), location.BannerTXD, location.BannerTexture);
                    }
                    PurchaseMenu menu = new PurchaseMenu(location, texture);
                    pool.Add(menu);
                    locations.Add(location, menu);
                }
            }
            else
            {
                Notification.Show($"~o~Warning~s~: Locations Directory was not found!");
            }

            // Finally, add the tick event and start working
            Tick += FoodShops_Tick_Init;
            Aborted += FoodShops_Aborted;
        }

        #endregion

        #region Tools

        private void DisableControls()
        {
            Game.DisableControlThisFrame(Control.LookUpDown);
            Game.DisableControlThisFrame(Control.LookLeftRight);

            Game.DisableControlThisFrame(Control.MoveLeftRight);
            Game.DisableControlThisFrame(Control.MoveUpDown);
            Game.DisableControlThisFrame(Control.MoveUpOnly);
            Game.DisableControlThisFrame(Control.MoveDownOnly);
            Game.DisableControlThisFrame(Control.MoveLeftOnly);
            Game.DisableControlThisFrame(Control.MoveRightOnly);
            Game.DisableControlThisFrame(Control.MoveUp);
            Game.DisableControlThisFrame(Control.MoveDown);
            Game.DisableControlThisFrame(Control.MoveLeft);
            Game.DisableControlThisFrame(Control.MoveRight);
        }

        #endregion

        #region Events

        private void FoodShops_Tick_Init(object sender, EventArgs e)
        {
            foreach (ShopLocation location in locations.Keys)
            {
                location.Initialize();
            }
            Tick -= FoodShops_Tick_Init;
            Tick += FoodShops_Tick_Run;
        }
        private void FoodShops_Tick_Run(object sender, EventArgs e)
        {
            // Process the contents of the menus and return if anything is open
            if (pool.AreAnyVisible)
            {
                DisableControls();
            }
            pool.Process();
            if (pool.AreAnyVisible)
            {
                DisableControls();
                return;
            }

            // Get some of the user's information to use it later
            Vector3 pos = Game.Player.Character.Position;

            // Iterate over the available interiors
            foreach (KeyValuePair<ShopLocation, PurchaseMenu> location in locations)
            {
                // If the player is too far from the location or is not on the correct interior, skip it
                if (pos.DistanceTo(location.Key.Trigger) > 50)
                {
                    continue;
                }

                // Otherwise, draw the marker in the correct position
                World.DrawMarker(MarkerType.VerticalCylinder, location.Key.Trigger, Vector3.Zero, Vector3.Zero, new Vector3(0.5f, 0.5f, 0.5f), Color.Red);

                // If the player is very close to the ma1rker, tell him to press interact
                if (pos.DistanceTo(location.Key.Trigger) < 1.25f)
                {
                    Screen.ShowHelpTextThisFrame("Press ~INPUT_CONTEXT~ to buy some food.");

                    if (Game.IsControlJustPressed(Control.Context))
                    {
                        location.Value.Visible = true;
                        return;
                    }
                }
            }
        }

        private void FoodShops_Aborted(object sender, EventArgs e)
        {
            Game.Player.CanControlCharacter = true;

            pool.HideAll();
            // Just in case HideAll() didn't worked
            Game.Player.Character.Opacity = 255;
            World.RenderingCamera = null;

            foreach (ShopLocation location in locations.Keys)
            {
                location.DoCleanup();
            }
        }

        #endregion
    }
}
