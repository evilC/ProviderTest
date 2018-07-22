using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace MIDI
{
    public class MidiProvider : IProvider
    {
        private readonly Dictionary<DeviceDescriptor, MidiDevice> _devices = new Dictionary<DeviceDescriptor, MidiDevice>();

        public ProviderReport GetInputList()
        {
            throw new NotImplementedException();
        }

        public IDisposable SubscribeBindMode(DeviceDescriptor deviceDescriptor, IObserver<InputReport> observer)
        {
            CreateDevice(deviceDescriptor);
            return _devices[deviceDescriptor].Subscribe(observer);
        }

        public void CreateDevice(DeviceDescriptor deviceDescriptor)
        {
            if (!_devices.ContainsKey(deviceDescriptor))
            {
                _devices.Add(deviceDescriptor, new MidiDevice(deviceDescriptor, DeviceEmptyEventHandler));
            }
        }

        private void DeviceEmptyEventHandler(object sender, DeviceEmptyEventArgs deviceEmptyEventArgs)
        {
            _devices[deviceEmptyEventArgs.DeviceDescriptor].Dispose();
            _devices.Remove(deviceEmptyEventArgs.DeviceDescriptor);
        }
    }
}
