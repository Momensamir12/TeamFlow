public class TemplateRenderer
{
    private readonly string _basePath;

    public TemplateRenderer(IWebHostEnvironment env)
    {
        _basePath = Path.Combine(env.ContentRootPath, "EmailTemplates");
    }

    public string Render(string templateName, Dictionary<string, string> values)
    {
        var path = Path.Combine(_basePath, $"{templateName}.html");
        var template = File.ReadAllText(path);

        foreach (var (key, value) in values)
            template = template.Replace($"{{{{{key}}}}}", value);

        return template;
    }
}
