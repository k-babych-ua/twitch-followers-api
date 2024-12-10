using Microsoft.AspNetCore.Mvc;

namespace TwitchFollowers.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Overview : ControllerBase
    {
        private IHttpClientFactory _httpClientFactory;

        public Overview(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<string>> Get(string username)
        {
            using HttpClient client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync($"https://tools.2807.eu/api/getfollows/{username}");

            return await response.Content.ReadAsStringAsync();
        }
    }
}
