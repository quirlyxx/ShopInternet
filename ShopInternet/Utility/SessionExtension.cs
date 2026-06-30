using System.Text.Json;

namespace ShopInternet.Utility;

public static class SessionExtension
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }  
    
    public static T? Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(value)!;
    }
}