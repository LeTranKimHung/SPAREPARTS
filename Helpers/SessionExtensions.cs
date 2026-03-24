using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace SparePartsManagement.Helpers;

public static class SessionExtensions
{
    public static void SetSession<T>(this Microsoft.AspNetCore.Http.ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetSession<T>(this Microsoft.AspNetCore.Http.ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
