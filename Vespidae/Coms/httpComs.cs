using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using VespidaeTools;
using Newtonsoft.Json;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;

namespace Coms
{
    public static class httpComs
    {
        public static async Task sendGcodeTask(List<String> code, string dir, string fileName, string ip)
        {
            string sendCode = string.Join("\n", code); 
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10); 
            client.BaseAddress = new Uri($"{ip}/");
            var stringContent = new StringContent(sendCode);

            var response = await client.PutAsync($"{dir}{fileName}.gcode", stringContent);
        }

        public static async Task<string> streamActions(List<VespidaeTools.Action> actions) {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>();

            var content = MakeJson(actions); 

            var response = await client.PostAsync("http://localhost:8080/fromClient", new StringContent(content,System.Text.Encoding.UTF8, "application/json"));

            var responseString = await response.Content.ReadAsStringAsync();
            return responseString; 
        }
        public static void streamGcodeCOM(List<String> code, string com, bool print)
        {
            string sendCode = string.Join("\n", code);
            string portname = com;
            SerialPort port = new SerialPort(portname, 115200);
            port.Open();
            int i = 0;
            while (print && i < code.Count)
            {
                if (i == 0) Debug.WriteLine("Print Starting");
                Debug.WriteLine(code[i]);
                port.WriteLine(code[i]);
                i++;
            }
            Debug.WriteLine("Print Finished");
            port.Close();
        }

        private static string MakeJson(List<VespidaeTools.Action> actions) {
            string package = JsonConvert.SerializeObject(actions, Formatting.Indented);

            return package;
        }
    }
}
