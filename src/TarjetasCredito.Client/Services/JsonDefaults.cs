using System.Text.Json;

namespace TarjetasCredito.Client.Services;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
