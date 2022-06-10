using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PonzianiSwissLib
{
    public class FederationUtil
    {
        public static async Task<List<Federation>?> GetFederations()
        {
            List<Federation>? result = null;
            try
            {
                HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(10);
                string url = "https://app.fide.com/api/v1/client/directory/federations?&q";
                result = await client.GetFromJsonAsync<List<Federation>>(url);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (result == null)
            {
                try
                {
                    result = JsonSerializer.Deserialize<List<Federation>>(Resource.federation_json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }          
            if (result != null)
            {
                foreach (var federation in result)
                {
                    if (federation.Id != null)
                        Federations.Add(federation.Id, $"{federation?.Name ?? string.Empty} ({federation?.Id})");
                }
            }
            return result;
        }

        public static KeyValuePair<string, string> GetFederation(string federation_key) => new(federation_key, Federations[federation_key]);

        public static Dictionary<string, string> Federations { private set; get; } = new() { { "FIDE", "Fide" } };
    }
    
    public class Federation
    {
        [JsonPropertyName("category")]
        public string? Category { set; get; }

        [JsonPropertyName("continent")]
        public string? Continent { set; get; }

        [JsonPropertyName("fed_short_name")]
        public string? Id { set; get; }

        [JsonPropertyName("fed_long_name")]
        public string? Name { set; get; }      
        
    }
}
