using Newtonsoft.Json;

namespace TwitchFollowers.Domain.Extensions
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            string stringContent = await content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(stringContent))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(stringContent);
        }
    }
}
