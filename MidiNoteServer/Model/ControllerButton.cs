using SharpDX.XInput;

namespace MidiNoteServer.Model
{
    class ControllerButton
    {
        public GamepadButtonFlags Flag { get; set; }

        public int MidiKey { get; set; }

        public bool LastState { get; set; } = false;
    }
}
