using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace DirectInput
{
    public class DiProvider
    {
        private readonly Dictionary<string, DiDevice> _devices = new Dictionary<string, DiDevice>();

        public IDisposable SubscribeInput(DeviceDescriptor deviceDescriptor, BindingDescriptor inputDescriptor, IObserver<InputModeReport> observer)
        {
            if (!DiWrapper.Instance.ConnectedDevices.ContainsKey(deviceDescriptor.DeviceHandle))
            {
                throw new Exception("Device not found");
            }

            if (!_devices.ContainsKey(deviceDescriptor.DeviceHandle))
            {
                _devices.Add(deviceDescriptor.DeviceHandle, new DiDevice(deviceDescriptor));
            }
            return _devices[deviceDescriptor.DeviceHandle].SubscribeInput(inputDescriptor, observer);
        }
    }
}
