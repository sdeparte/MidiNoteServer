
using MidiNoteServer.Service;
using System;

namespace MidiNoteServer
{
    class Program
    {
        private const int PORT = 8080;

        static void Main(string[] args)
        {
            Console.WriteLine(" +-------------------------------------------------------+");
            Console.WriteLine(" |                   Midi Note Server                    | ");
            Console.WriteLine(" +-------------------------------------------------------+");
            Console.WriteLine($" |           Base URL : http://localhost:{PORT}/           |");
            Console.WriteLine(" +-------------------------------------------------------+");
            Console.WriteLine(" |        Press 'q' to stop Server and close APP         | ");
            Console.WriteLine(" +-------------------------------------------------------+\n");

            HttpServer httpServer = new HttpServer($"http://+:{PORT}/");
            httpServer.Start();

            while (Console.ReadKey(true).KeyChar != 'q');

            httpServer.Stop();
        }
    }
}
