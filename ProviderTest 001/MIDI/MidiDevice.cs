using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using NAudio.Midi;

namespace MIDI
{
    class MidiDevice : InputDevice<MidiInMessageEventArgs, (int, int)>
    {
        private MidiIn _device;

        public MidiDevice(DeviceDescriptor deviceDescriptor, EventHandler<DeviceEmptyEventArgs> deviceEmptyEventHandler) : base(deviceDescriptor, deviceEmptyEventHandler)
        {
            var numberOfDevices = MidiIn.NumberOfDevices;
            var instance = deviceDescriptor.DeviceInstance;
            for (var i = 0; i < numberOfDevices; i++)
            {
                var devInfo = MidiIn.DeviceInfo(i);
                if (devInfo.ProductName != deviceDescriptor.DeviceHandle) continue;

                if (instance > 0)
                {
                    instance--;
                    continue;
                }
                _device = new MidiIn(i);
                _device.MessageReceived += MidiIn_MessageReceived;
                _device.Start();
                break;
            }
        }

        public void MidiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            // 128 = NoteOff, 144 = NoteOn, 176 = ControlChange, 224 - PitchWheelChanged
            var code = (int)e.MidiEvent.CommandCode;
            int index;
            int value;
            switch (code)
            {
                case 224:
                    var pw = (PitchWheelChangeEvent) e.MidiEvent;
                    index = 0;
                    value = pw.Pitch;
                    break;
                case 176:
                    // ControlChange
                    var cc = (ControlChangeEvent)e.MidiEvent;
                    index = (int)cc.Controller;
                    value = cc.ControllerValue;
                    break;
                case 128:
                case 144:
                    // NoteOn / NoteOff
                    var ne = (NoteEvent) e.MidiEvent;
                    index = ne.NoteNumber;
                    value = code == 128 ? 0 : ne.Velocity;
                    break;
                default:
                    Console.WriteLine($"Skipping code {code}");
                    return;
            }
            var channel = e.MidiEvent.Channel;
            //var index = code == 1 ? x.Controller
            Console.WriteLine($"Channel: {channel}, Event: {code}, Index: {index}, Value: {value}");
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override (int, int) GetPollProcessorKey(BindingDescriptor bindingDescriptor)
        {
            throw new NotImplementedException();
        }
    }
}
