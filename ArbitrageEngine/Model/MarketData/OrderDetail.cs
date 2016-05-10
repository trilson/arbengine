namespace ArbitrageEngine.Model
{
    public class OrderDetail
    {
        public decimal Price { get; private set; }
        public decimal Quantity { get; private set; }

        public OrderDetail(decimal price, decimal quantity)
        {
            Price = price;
            Quantity = quantity;
        }
    }
}