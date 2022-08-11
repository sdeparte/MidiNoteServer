using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MidiNoteServer.Service
{
    class HttpServer
    {
        private const int _handlerThread = 2;

        private readonly MidiController _midiController;
        private readonly HttpListener _listener;

        public bool IsStarted { get { return _listener.IsListening; } }

        public HttpServer(MidiController midiController, string url)
        {
            _midiController = midiController;

            _listener = new HttpListener();
            _listener.Prefixes.Add(url);
        }

        public void Start()
        {
            Console.WriteLine($" [{DateTime.Now}] HTTP Server : Starting ...");

            if (_listener.IsListening)
                return;

            _listener.Start();

            for (int i = 0; i < _handlerThread; i++)
            {
                _listener.GetContextAsync().ContinueWith(ProcessRequestHandlerAsync);
            }

            Console.WriteLine($" [{DateTime.Now}] HTTP Server : Started");
        }

        public void Stop()
        {
            if (_listener.IsListening)
                _listener.Stop();
        }

        private async Task ProcessRequestHandlerAsync(Task<HttpListenerContext> result)
        {
            if (!_listener.IsListening)
                return;

            var context = result.Result;

            // Start new listener which replace this
            _listener.GetContextAsync().ContinueWith(ProcessRequestHandlerAsync);


            // Read RequestBody
            string requestBody = new StreamReader(context.Request.InputStream).ReadToEnd();

            // Prepare response
            Stream output = context.Response.OutputStream;

            Console.WriteLine($" [{DateTime.Now}] - HTTP Request : {context.Request.RawUrl}");

            switch (context.Request.Url.LocalPath)
            {
                case "/forwardNote":
                    string note = null;

                    switch (context.Request.HttpMethod)
                    {
                        case "POST":
                            note = HttpUtility.ParseQueryString(requestBody).Get("midiNote");
                            break;
                        case "GET":
                            note = context.Request.QueryString.Get("midiNote");
                            break;
                    }

                    if (note == null)
                    {
                        Write404ErrorHeader(context.Response);
                        return;
                    }

                    _ = _midiController.SendMidiNoteAsync(int.Parse(note));

                    Write200Header(context.Response);
                    return;

                default:
                    Write404ErrorHeader(context.Response);
                    return;
            }
        }

        public void Write200Header(HttpListenerResponse response)
        {
            Stream output = response.OutputStream;

            WriteLine(response, "HTTP/1.0 200");

            output.Flush();
            output.Close();

            Console.WriteLine($" [{DateTime.Now}] - HTTP Response : 200");
        }

        public void Write404ErrorHeader(HttpListenerResponse response)
        {
            Stream output = response.OutputStream;

            WriteLine(response, "HTTP/1.0 404 NotFound");

            output.Flush();
            output.Close();

            Console.WriteLine($" [{DateTime.Now}] - HTTP Response : 404");
        }

        private void WriteLine(HttpListenerResponse response, string text)
        {
            byte[] data = BytesOf(text + "\r\n");
            response.OutputStream.Write(data, 0, data.Length);
        }

        private static byte[] BytesOf(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }
    }
}
