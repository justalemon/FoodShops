using LemonUI.Menus;

namespace FoodShops
{
    /// <summary>
    /// The menu used to purchase things.
    /// </summary>
    public class PurchaseMenu : NativeMenu
    {
        #region Constructor

        public PurchaseMenu(ShopLocation location) : base("", location.Name)
        {
            //NoItemsText = "This Shop does not has any food or drinks to be purchased.";
            foreach (ShopMenu menu in location.Menus)
            {
                foreach (ShopMeal meal in menu.Meals)
                {
                    Add(new PurchaseItem(meal));
                }
            }
        }

        #endregion
    }
}
