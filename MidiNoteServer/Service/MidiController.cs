using RtMidi.Core;
using RtMidi.Core.Devices;
using RtMidi.Core.Devices.Infos;
using RtMidi.Core.Enums;
using RtMidi.Core.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MidiNoteServer.Service
{
    class MidiController
    {
        private List<IMidiOutputDevice> _outputDevices = new List<IMidiOutputDevice>();

        public MidiController()
        {
            foreach (IMidiOutputDeviceInfo device in MidiDeviceManager.Default.OutputDevices)
            {
                if (!device.Name.Contains("Microsoft GS Wavetable Synth"))
                {
                    IMidiOutputDevice outputDevice = device.CreateDevice();
                    outputDevice.Open();
                    _outputDevices.Add(outputDevice);
                }
            }
        }

        public void Dispose()
        {
            foreach (IMidiOutputDevice outputDevice in _outputDevices)
            {
                outputDevice.Close();
            }
        }

        public async Task SendMidiNoteAsync(int midiNote)
        {
            SendMidiNote(midiNote, 127);

            SendMidiNote(midiNote, 0);
        }

        public void SendMidiNote(int midiNote, int velocity)
        {
            foreach (IMidiOutputDevice outputDevice in _outputDevices)
            {
                outputDevice.Send(new NoteOnMessage(Channel.Channel1, (Key)midiNote, velocity));
            }

            Console.WriteLine($" [{DateTime.Now}] -- Midi Controller : Note #{midiNote}:{velocity} sended");
        }
    }
}
