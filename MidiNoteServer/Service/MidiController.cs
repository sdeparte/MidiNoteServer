using RtMidi.Core;
using RtMidi.Core.Devices;
using RtMidi.Core.Devices.Infos;
using RtMidi.Core.Enums;
using RtMidi.Core.Messages;
using System;
using System.Threading.Tasks;

namespace MidiNoteServer.Service
{
    class MidiController
    {
        public void UpMidiNote(int midiNote)
        {
            foreach (IMidiOutputDeviceInfo device in MidiDeviceManager.Default.OutputDevices)
            {
                if (device.Name.Contains("Arduino"))
                {
                    _ = SendMidiNoteAsync(device, midiNote);
                }
            }

            Console.WriteLine($" [{DateTime.Now}] -- Midi Controller : Note #{midiNote} sended");
        }


        private async Task SendMidiNoteAsync(IMidiOutputDeviceInfo device, int midiNote)
        {
            IMidiOutputDevice outputDevice = device.CreateDevice();

            outputDevice.Open();

            outputDevice.Send(new NoteOnMessage(Channel.Channel1, (Key)midiNote, 127));

            await Task.Delay(100);

            outputDevice.Send(new NoteOffMessage(Channel.Channel1, (Key)midiNote, 0));

            await Task.Delay(5000);

            outputDevice.Send(new NoteOnMessage(Channel.Channel1, (Key)midiNote, 127));

            await Task.Delay(100);

            outputDevice.Send(new NoteOffMessage(Channel.Channel1, (Key)midiNote, 0));

            outputDevice.Close();
        }
    }
}
