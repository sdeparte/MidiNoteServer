namespace MidiNoteServer.Service
{
    internal class Point
    {
        public float X { get; set; }
        public float Y { get; set; }
        
        public Point(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}