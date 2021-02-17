using GTA.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace FoodShops.Converters
{
    /// <summary>
    /// Loads the 
    /// </summary>
    public class ShopMenuConverter : JsonConverter<List<ShopMenu>>
    {
        private readonly List<ShopMenu> menus;

        public ShopMenuConverter(List<ShopMenu> menus)
        {
            this.menus = menus;
        }

        public override List<ShopMenu> ReadJson(JsonReader reader, Type objectType, List<ShopMenu> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JArray values = JArray.Load(reader);
            List<ShopMenu> foundMenus = new List<ShopMenu>();

            foreach (JToken value in values)
            {
                bool added = false;
                Guid guid = Guid.Parse((string)value);
                foreach (ShopMenu menu in menus)
                {
                    if (menu.ID == guid)
                    {
                        foundMenus.Add(menu);
                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    Notification.Show($"~o~Warning~s~: Menu with the ID {guid} was not found!");
                }
            }

            return foundMenus;
        }

        public override void WriteJson(JsonWriter writer, List<ShopMenu> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
