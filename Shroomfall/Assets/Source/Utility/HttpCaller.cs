using Contract.DTO.Abstraction;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.Utility
{
    public class VoidResponse
    {

    }

    public static class HttpCaller
    {
        #region Attributes
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri(Configuration.BASE_URL),
            Timeout = TimeSpan.FromSeconds(15)
        };
        #endregion

        #region Properties
        #endregion

        #region Methods
        public static async Task<TResponse> GetAsync<TResponse>(
            string endpoint)
        {
            var response = await client.GetAsync(endpoint);
            return await ParseResponse<TResponse>(response);
        }

        public static async Task<TResponse> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest dto)
        {
            var response = await client.PostAsync(endpoint, CreateJsonContent(dto));
            return await ParseResponse<TResponse>(response);
        }

        public static async Task<TResponse> PutAsync<TRequest, TResponse>(
            string endpoint,
            TRequest dto)
        {
            var response = await client.PutAsync(endpoint, CreateJsonContent(dto));
            return await ParseResponse<TResponse>(response);
        }

        public static async Task<TResponse> DeleteAsync<TResponse>(
            string endpoint)
        {
            var response = await client.DeleteAsync(endpoint);
            return await ParseResponse<TResponse>(response);
        }

        public static void SetBearerToken(
            string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static void ClearBearerToken()
        {
            client.DefaultRequestHeaders.Authorization = null;
        }

        private static async Task<TResponse> ParseResponse<TResponse>(
            HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Fallback text if something is totally broken
                string displayMessage = "An unexpected connection error occurred.";

                try
                {
                    // Deserialize the structured API Error payload
                    var error = JsonConvert.DeserializeObject<ApiErrorDTO>(body);

                    if (error != null && !string.IsNullOrWhiteSpace(error.Code))
                    {
                        // Try to pass the unique code key into our localization map
                        displayMessage = UILocalizationTable.Get(error.Code);
                    }
                    else if (error != null && !string.IsNullOrWhiteSpace(error.Message))
                    {
                        // Fallback to raw message if code isn't provided
                        displayMessage = error.Message;
                    }
                }
                catch (Exception jsonEx)
                {

                }

                // Throw the localized message string up the call stack
                throw new Exception(displayMessage);
            }

            if (typeof(TResponse) == typeof(VoidResponse))
                return default;

            if (string.IsNullOrWhiteSpace(body))
                return default;

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new ComponentInstanceConverter() }
            };

            return JsonConvert.DeserializeObject<TResponse>(body, settings);
        }

        private static StringContent CreateJsonContent<T>(
            T dto)
        {
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var json = JsonConvert.SerializeObject(dto, settings);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
        #endregion
    }
}
