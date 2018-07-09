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

        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputModeReport> observer)
        {
            if (!DiWrapper.Instance.ConnectedDevices.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
            {
                throw new Exception("Device not found");
            }

            if (!_devices.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
            {
                _devices.Add(subReq.DeviceDescriptor.DeviceHandle, new DiDevice(subReq.DeviceDescriptor));
            }
            return _devices[subReq.DeviceDescriptor.DeviceHandle].SubscribeInput(subReq, observer);
        }
    }
}
