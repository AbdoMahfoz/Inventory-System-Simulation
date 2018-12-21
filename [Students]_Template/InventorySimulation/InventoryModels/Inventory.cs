namespace InventoryModels
{
    public class Inventory
    {
        public int Quantity;
        public Order CurrentOrder;
        public Inventory()
        {
            Quantity = 0;
            CurrentOrder = null;
        }
    }
}
