using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using censudex_api_gateway.src.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace censudex_api_gateway.src.Controller
{
    [ApiController]
    
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Inject HttpClientFactory
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;
        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            /// <summary>
            /// create http client and copy auth header
            /// </summary>
            /// <returns></returns>
            var client = _httpClientFactory.CreateClient("AuthService");
            CopyAuthHeaderToClient(client);

            var response = await client.PostAsJsonAsync("api/login/login", loginDto);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new ContentResult
                {
                    Content = content,
                    ContentType = "application/json",
                    StatusCode = (int)response.StatusCode
                };
            }

            return StatusCode((int)response.StatusCode, content);
        }
        /// <summary>
        /// Validate token
        /// </summary>
        /// <param name="validateTokenDto"></param>
        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidatetokenDto validateTokenDto)
        {
            /// <summary>
            /// create http client and copy auth header
            /// </summary>
            /// <returns></returns>
            var client = _httpClientFactory.CreateClient("AuthService");
            CopyAuthHeaderToClient(client);
            /// <summary>
            /// remove existing Authorization header and add new one if not present in request headers
            /// </summary>
            /// <param name="!string.IsNullOrWhiteSpace(validateTokenDto?.Token)"></param>
            /// <returns></returns>
            if (!Request.Headers.ContainsKey("Authorization") && !string.IsNullOrWhiteSpace(validateTokenDto?.Token))
            {
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {validateTokenDto.Token}");
            }

            var response = await client.GetAsync("api/login");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return new ContentResult { Content = content, ContentType = "application/json", StatusCode = (int)response.StatusCode };

            return StatusCode((int)response.StatusCode, content);
        }

        /// <summary>
        /// Logout user
        /// </summary>
        /// <param name="logoutDto"></param>
        /// <returns></returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
        {
            var client = _httpClientFactory.CreateClient("AuthService");

            CopyAuthHeaderToClient(client);

            
            if (!Request.Headers.ContainsKey("Authorization") && !string.IsNullOrWhiteSpace(logoutDto?.Token))
            {
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {logoutDto.Token}");
            }

            
            if (!client.DefaultRequestHeaders.Contains("Authorization"))
            {
                return BadRequest(new { error = "Authorization header missing. Send 'Authorization: Bearer <token>' or include { token } in body." });
            }

            var response = await client.PostAsJsonAsync("api/login/logout", logoutDto);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode) return new ContentResult { Content = content, ContentType = "application/json", StatusCode = (int)response.StatusCode };
            return StatusCode((int)response.StatusCode, content);
        }

        /// <summary>
        /// Copy Authorization header from incoming request to HttpClient
        /// </summary>
        /// <param name="client"></param>
        private void CopyAuthHeaderToClient(System.Net.Http.HttpClient client)
        {
            if (Request.Headers.TryGetValue("Authorization", out var authValues))
            {
                var auth = authValues.ToString();
                if (!string.IsNullOrEmpty(auth))
                {
                    client.DefaultRequestHeaders.Remove("Authorization");
                    client.DefaultRequestHeaders.Add("Authorization", auth);
                }
            }
        }
    }
}