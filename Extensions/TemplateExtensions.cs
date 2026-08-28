namespace JobAlertFilter.Extensions;

public static class TemplateExtensions
{
    public static Dictionary<string, string> ToReplacements<T>(this T profile)
        where T : class, new()
    {
        var dict = new Dictionary<string, string>();
        var props = typeof(T).GetProperties();

        foreach (var prop in props)
        {
            var value = prop.GetValue(profile);
            var text = value switch
            {
                null => "",
                IEnumerable<string> list => string.Join(", ", list),
                _ => value.ToString() ?? ""
            };
            dict[prop.Name] = text;
        }

        return dict;
    }
}