using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic; 

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

    }
}
