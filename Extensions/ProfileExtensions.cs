using JobAlertFilter.Options;

namespace JobAlertFilter.Extensions;

public static class ProfileExtensions
{
    public static Dictionary<string, string> ToReplacements(this ProfileOptions profile)
    {
        var dict = new Dictionary<string, string>();
        var props = typeof(ProfileOptions).GetProperties();

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