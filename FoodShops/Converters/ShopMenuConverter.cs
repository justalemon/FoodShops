using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FoodShops.Converters
{
    /// <summary>
    /// Loads the 
    /// </summary>
    public class ShopMenuConverter : JsonConverter<ShopMenu>
    {
        private readonly List<ShopMenu> menus;

        public ShopMenuConverter(List<ShopMenu> menus)
        {
            this.menus = menus;
        }

        public override ShopMenu ReadJson(JsonReader reader, Type objectType, ShopMenu existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Get the ID and try to parse it as JSON
            string id = (string)reader.Value;
            Guid guid = Guid.Parse(id);

            // Then, try to find any menus with the same ID
            foreach (ShopMenu menu in menus)
            {
                if (menu.ID == guid)
                {
                    return menu;
                }
            }
            // If none were found, return null
            return null;
        }

        public override void WriteJson(JsonWriter writer, ShopMenu value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
