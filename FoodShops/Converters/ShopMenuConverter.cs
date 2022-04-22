using GTA.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace FoodShops.Converters
{
    /// <summary>
    /// Loads the list of Shop Menus for a specific Location.
    /// </summary>
    public class ShopMenuConverter : JsonConverter<List<ShopMenu>>
    {
        private readonly Dictionary<Guid, ShopMenu> menus;

        public ShopMenuConverter(Dictionary<Guid, ShopMenu> menus)
        {
            this.menus = menus;
        }

        public override List<ShopMenu> ReadJson(JsonReader reader, Type objectType, List<ShopMenu> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JArray values = JArray.Load(reader);
            List<ShopMenu> foundMenus = new List<ShopMenu>();

            foreach (JToken value in values)
            {
                Guid guid = Guid.Parse((string)value);
                if (menus.ContainsKey(guid))
                {
                    foundMenus.Add(menus[guid]);
                }
                else
                {
                    Notification.Show($"~o~Warning~s~: Menu with ID {guid} was not found!");
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
