using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using VespidaeTools;
using Newtonsoft.Json;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Linq;

namespace Coms
{
    public class HttpComs
    {
        private volatile bool stopStreaming;
        private SerialPort port;

        public HttpComs()
        {
            port = new SerialPort();
            stopStreaming = false;
            
        }

        public void openCOM(string com, int baudRate)
        {
            port.BaudRate = baudRate;
            port.PortName = com;
            port.DtrEnable = true;
            port.RtsEnable = true;
            port.Open();
            Debug.WriteLine("Opening port");
            System.Threading.Thread.Sleep(3000);
            port.DiscardInBuffer();
            if (port.IsOpen)
            {
                Debug.WriteLine(String.Format("COM{0}: Port is opened", com));

            }
        }

        public void closeCOM()
        {
            port.Close();
            Debug.WriteLine(String.Format("COM{0}: Port is closed", port.PortName));
        }

        public static string GetDirectoryListingRegexForUrl(string url)
        {
            return "<a href=\".*\">(?<name>.*)</a>";
        }

        public async Task sendGcodeTask(List<String> code, string dir, string fileName, string ip)
        {
            string sendCode = string.Join("\n", code); 
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10); 
            client.BaseAddress = new Uri($"{ip}/");
            var stringContent = new StringContent(sendCode);

            var response = await client.PutAsync($"{dir}{fileName}.gcode", stringContent);
        }

        public async Task<string> streamActions(List<VespidaeTools.Action> actions) {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>();

            var content = MakeJson(actions); 

            var response = await client.PostAsync("http://localhost:8080/fromClient", new StringContent(content,System.Text.Encoding.UTF8, "application/json"));

            var responseString = await response.Content.ReadAsStringAsync();
            return responseString; 
        }
        public void streamGcodeCOM(List<String> code, string com, bool print)
        {
            if (!port.IsOpen)
            {
                Console.WriteLine("Serial port is not open.");
                return;
            }

            Debug.WriteLine("Is there anything here");

            string sendCode = string.Join("\n", code);
            string[] buffer = new string[] { };
            int i = 0;
            DateTime timer = DateTime.Now;
            TimeSpan diff;
            bool wait = false;

            var cleanedCode = cleanCode(code);
            Debug.WriteLine("Code has been cleaned");

            while (print && i < cleanedCode.Count)
            {
                AutoResetEvent okReceived = new AutoResetEvent(false);
                if (i == 0) Debug.WriteLine("Print Starting");

                // checks if the buffer has space for more outputs, else wait
                var string_bytes = cleanedCode[i].Length * sizeof(char);
                while (port.BytesToWrite + string_bytes > port.WriteBufferSize - 10)
                {
                    wait = true;
                    System.Threading.Thread.Sleep(200); // Wait for buffer space
                    timer = System.DateTime.Now;
                    Debug.WriteLine(String.Format("Thread started waiting at {0}", timer.ToString("hh:mm:ss.fff")));
                }    
                if (wait)
                {
                    diff = System.DateTime.Now - timer;
                    timer = System.DateTime.Now;
                    Debug.WriteLine(String.Format("Thread stopped waiting at {0}. Wait time was {1}", timer.ToString("hh:mm:ss.fff"), diff));
                }

                if (stopStreaming)
                {
                    Debug.WriteLine("Streaming Interrupted");
                    port.Close();
                }

                try
                {
                    if (cleanedCode[i].Length > 0 && (cleanedCode[i][0] == (char)'G' || cleanedCode[i][0] == 'M'))
                    {

                        Debug.WriteLine(cleanedCode[i]);
                        port.WriteLine(cleanedCode[i]);
                    }
                    else
                    {
                        Debug.WriteLine(cleanedCode[i] + "Skip send");
                    }
                    bool received = false;

                    // Wait for OK before sending next
                    if (cleanedCode[i].Contains("G28") || cleanedCode[i].Contains("G92") || cleanedCode[i].Contains("G92") || cleanedCode[i].Contains("M104") || cleanedCode[i].Contains("M140"))
                    {
                        received = true;
                        Debug.WriteLine("Just skipping the setup steps");
                    }
                    else
                    {
                        received = okReceived.WaitOne(5000); // 15 seconds timeout
                    }
                    if (!received)
                    {
                        Debug.WriteLine("Timeout waiting for OK. Aborting...");
                        break;
                    }

                    // This will throw a TimeoutException if no data is received within the timeout
                    string response = port.ReadLine();
                    Trace.WriteLine($"Got response: {response}");
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("Read took longer than expected");
                }
                // Catch the IOException generated if the
                // specified part of the file is locked.
                catch (IOException e)
                {
                    Console.WriteLine(
                        "{0}: The write operation could not " +
                        "be performed because the specified " +
                        "part of the file is locked.",
                        e.GetType().Name);
                }
                catch
                {
                    Trace.WriteLine("Failed to write to port");
                }

                i++;
            }

            Debug.WriteLine("Print Finished");
            port.Close();
        }

        private List<string> cleanCode(List<string> code)
        {
            List<string> cleanedCode = new List<string>();
            foreach (var line in code)
            {
                if (line[0] == 'G' || line[0] == 'M')
                {
                    cleanedCode.Add(line);
                }
            }
            return cleanedCode;
        }

        public void StopStreaming()
        {
            stopStreaming = true;
        }
        public void Close()
        {
            if (port.IsOpen)
            {
                port.Close();
            }
        }

        private static string MakeJson(List<VespidaeTools.Action> actions) {
            string package = JsonConvert.SerializeObject(actions, Formatting.Indented);

            return package;
        }
    }

    public class IPComs
    {
        private string ip_address;
        public IPComs(string address)
        {
            ip_address = address;
        }
        public async Task sendGcodeTask(List<String> code, string dir, string fileName, string ip)
        {
            string sendCode = string.Join("\n", code);
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.BaseAddress = new Uri($"{ip}/");
            var stringContent = new StringContent(sendCode);

            var response = await client.PutAsync($"{dir}{fileName}.gcode", stringContent);
        }

        public async Task<string> streamActions(List<VespidaeTools.Action> actions)
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>();

            var content = MakeJson(actions);

            var response = await client.PostAsync("http://localhost:8080/fromClient", new StringContent(content, System.Text.Encoding.UTF8, "application/json"));

            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public string GetDirectoryListingRegexForUrl(string url)
        {
            return "<a href=\".*\">(?<name>.*)</a>";
        }

        private static string MakeJson(List<VespidaeTools.Action> actions)
        {
            string package = JsonConvert.SerializeObject(actions, Formatting.Indented);

            return package;
        }
    }
    public class COMStreamer
    {
        private SerialPort serialPort;
        private Queue<string> gcodeQueue;
        private AutoResetEvent okReceived;
        private StringBuilder receivedBuffer;
        private bool running;

        public COMStreamer(string portName, int baudRate = 115200)
        {
            serialPort = new SerialPort(portName, baudRate)
            {
                NewLine = "\n",
                ReadTimeout = 500,
                WriteTimeout = 500,
                DtrEnable = true,
                RtsEnable = true
            };
            serialPort.DataReceived += SerialDataReceivedHandler;

            okReceived = new AutoResetEvent(false);
            receivedBuffer = new StringBuilder();
        }

        // Connects to the serial port at the specified COM port and baud rate
        public void Connect()
        {
            serialPort.Open();
            Debug.WriteLine(String.Format("{0}: Port is opened", serialPort.PortName));
            Debug.WriteLine("Connected to printer. Waiting for printer to be ready...");
            Thread.Sleep(3000);
            FlushStartupMessages();
        }

        // Printing action
        public void Print(List<string> gcodeCommands)
        {
            var cleanedCommands = cleanCode(gcodeCommands); // cleans the gcode commands of anything that isn't an actual G-code command

            List<string> startup = new List<string> { "G28", "G92", "M104", "M140", "M109", "M190" }; // code that needs extra time to run
            gcodeQueue = new Queue<string>(cleanedCommands);
            running = true;

            // goes throughg the queue of commands to run
            while (running && gcodeQueue.Count > 0)
            {
                string command = gcodeQueue.Dequeue();
                Debug.WriteLine($">> {command}");
                serialPort.WriteLine(command);

                // Wait for OK with timeout
                int waitTimeMs = 10000;
                if (startup.Any(s=>command.Contains(s)))
                {
                    Debug.WriteLine("Changed Start time");
                    waitTimeMs = 180000; // for startup commands, set max wait time to 3 minutes
                }
                
                // continues to check for OK commands every 100 ms until it is received
                int waited = 0;
                int sleepStep = 100;
                while (!okReceived.WaitOne(1))
                {
                    Thread.Sleep(sleepStep);
                    waited += sleepStep;
                    if (waited >= waitTimeMs)
                    {
                        Debug.WriteLine("Timeout waiting for OK. Aborting...");
                        running = false;
                        break;
                    }
                }
            }

            // this is what happens at the end after all commands have been streamed
            if (running)
            {
                Thread.Sleep(10000);
                Debug.WriteLine("Finished sending G-code!");
            }
                
            else
                Debug.WriteLine("Print aborted.");
        }

        public void Disconnect()
        {
            Debug.WriteLine("Disconnecting...");
            running = false;
            Debug.WriteLine(String.Format("Is Printer Open? {0}", serialPort.IsOpen.ToString()));
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                Debug.WriteLine("Disconnected from printer.");
            }
            else
            {
                Debug.WriteLine("COM is already closed!");
            }
        }

        // Gets rid of any startup messages received so they don't cause read errors for the stream
        private void FlushStartupMessages()
        {
            Debug.WriteLine("Flushing Startup Messages");
            serialPort.DiscardInBuffer();
        }

        private void SerialDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string incoming = serialPort.ReadExisting();
                receivedBuffer.Append(incoming);

                while (receivedBuffer.ToString().Contains("\n"))
                {
                    int newlineIndex = receivedBuffer.ToString().IndexOf("\n");
                    string line = receivedBuffer.ToString(0, newlineIndex).Trim();
                    receivedBuffer.Remove(0, newlineIndex + 1);

                    if (string.IsNullOrWhiteSpace(line)) continue;
                    Console.WriteLine($"<< {line}");

                    if (line.StartsWith("ok") || line.Contains("ok"))
                    {
                        okReceived.Set();
                    }
                }
            }
            catch (TimeoutException)
            {
                // Ignore read timeout
            }
        }

        // Removes any non-GCode lines (e.g. empty lines, comments, etc.)
        private List<string> cleanCode(List<string> code)
        {
            List<string> cleanedCode = new List<string>();
            foreach (var line in code)
            {
                if (line.Length > 0 && (line[0] == 'G' || line[0] == 'M'))
                {
                    cleanedCode.Add(line);
                }
            }
            return cleanedCode;
        }

        // Disposes of the COM port
        public void Dispose()
        {
            running = false;
            okReceived?.Dispose();
            if (serialPort.IsOpen)
                serialPort.Close();
            serialPort.Dispose();
        }
    }
}
