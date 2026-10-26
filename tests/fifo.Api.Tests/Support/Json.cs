using System.Net.Http.Json;
using System.Text.Json;

namespace fifo.Api.Tests.Support;

public static class Json
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var value = await response.Content.ReadFromJsonAsync<T>(Options);
        Assert.NotNull(value);
        return value;
    }
}
