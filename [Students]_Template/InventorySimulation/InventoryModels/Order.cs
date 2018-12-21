namespace InventoryModels
{
    public class Order
    {
        public int Quantity, DueDate;
        public Order()
        {
            Quantity = DueDate = 0;
        }
    }
}
