namespace PromoTex.DTO.Order
{
    public class CreateOrderDTO
    {
        public string ShippingAddress { get; set; }

        public List<OrderProductDTO> Products { get; set; }
    }
}
