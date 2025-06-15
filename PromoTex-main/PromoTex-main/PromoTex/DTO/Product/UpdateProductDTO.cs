namespace PromoTex.DTO.Product
{
    public class UpdateProductDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public IFormFile? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public int Quantity { get; set; }
        public string CategoryName { get; set; }
        public string StoreName { get; set; }
        public List<string> Colors { get; set; }
        public List<string> Sizes { get; set; }
    }
}
