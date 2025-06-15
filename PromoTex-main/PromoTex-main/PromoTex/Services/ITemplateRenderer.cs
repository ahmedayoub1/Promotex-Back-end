namespace PromoTex.Services
{
    public interface ITemplateRenderer
    {
        Task<string> RenderTemplateAsync(string filePath, Dictionary<string, string> placeholders);
    }
}
