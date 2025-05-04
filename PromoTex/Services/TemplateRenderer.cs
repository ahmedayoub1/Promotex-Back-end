using Microsoft.Build.Framework;

namespace PromoTex.Services
{
    public class TemplateRenderer : ITemplateRenderer
    {
        public async Task<string> RenderTemplateAsync(string filePath, Dictionary<string, string> placeholders)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Template not found", filePath);

            var content = await File.ReadAllTextAsync(filePath);

            foreach (var kv in placeholders)
            {
                content = content.Replace($"{{{{{kv.Key}}}}}", kv.Value);
            }

            return content;
        }
    }

}
