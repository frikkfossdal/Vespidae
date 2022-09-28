using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using VespidaeTools;
using Newtonsoft.Json; 

namespace Coms
{
    public static class httpComs
    {
        public static async Task sendGcodeTask(List<String> code, string fileName, string ip)
        {
            string sendCode = string.Join("\n", code); 
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10); 
            client.BaseAddress = new Uri($"{ip}/machine/");
            var stringContent = new StringContent(sendCode);

            var response = await client.PutAsync($"file/gcodes/Vespidae/{fileName}.gcode", stringContent);
        }

        public static async Task<string> streamActions(List<VespidaeTools.Action> actions) {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>();

            var content = MakeJson(actions); 

            var response = await client.PostAsync("http://localhost:8080/fromClient", new StringContent(content,System.Text.Encoding.UTF8, "application/json"));

            var responseString = await response.Content.ReadAsStringAsync();
            return responseString; 
        }

        private static string MakeJson(List<VespidaeTools.Action> actions) {
            string package = JsonConvert.SerializeObject(actions, Formatting.Indented);

            return package;
        }
    }
}
