using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace TestingAuth.Sample.Api.Get
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json",true,true)
                .Build();

            var TestingServiceBaseUrl = Environments.Test.BaseUrl;
            var TestingServiceScope = Environments.Test.Scope;

            var tenantId = configuration["ClientApplication:TenantId"];
            var clientId = configuration["ClientApplication:ClientId"];
            var clientSecret = configuration["ClientApplication:ClientSecret"];
            var b2Cauthority = configuration["ClientApplication:b2Cauthority"];           

            var token = await GetAccessToken(tenantId, clientId, clientSecret, b2Cauthority, TestingServiceScope);
            var client = GetHttpClient(token);

            await GetLabOrders(client, TestingServiceBaseUrl);

        }

        private static async Task<string> GetAccessToken(string tenantId, string clientId, string clientSecret,string b2Cauthority, params string[] scopes)
        {
            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithAuthority(b2Cauthority)
                .WithTenantId(tenantId)
                .WithClientSecret(clientSecret)
                .Build();

            var result = await app.AcquireTokenForClient(scopes)
                .ExecuteAsync();

            return result.AccessToken;
        }

        private static HttpClient GetHttpClient(string accessToken)
        {
            // Use HttpClientFactory when using this for real
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return client;
        }

        private static async Task GetLabOrders(HttpClient client, string baseUrl) 
        {
           // var response = await client.GetAsync($"{baseUrl}/LabGlu/customers.xml");
            var response = await client.GetAsync($"{baseUrl}/laborder");

            //var responseInfo1 = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var responseInfo = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseInfo);
        }

    public static class Environments
    {
        public static Environment Production = new Environment
        {
            BaseUrl = "https://api.veritas.healthcare",
            Scope = "https://veritastestingb2c.onmicrosoft.com/1d22859f-da06-4709-bb33-0bab942d8443/.default"
        };
        public static Environment Test = new Environment
        {
            BaseUrl = "https://test.api.veritas.healthcare",
            Scope = "https://veritasvaxtestb2c.onmicrosoft.com/d7cf4f44-ce43-4fd9-9da2-ba88ebb6e6b2/.default"
        };
    }
    public class Environment
    {
        public string BaseUrl { get; set; }
        public string Scope { get; set; }
    }
    }

}
