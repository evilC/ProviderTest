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

        // ToDo: Add support for device instances
        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputModeReport> observer)
        {
            if (!DiWrapper.Instance.ConnectedDevices.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
            {
                throw new Exception("Device not found");
            }

            CreateDevice(subReq.DeviceDescriptor);
            return _devices[subReq.DeviceDescriptor.DeviceHandle].SubscribeInput(subReq, observer);
        }

        public void CreateDevice(DeviceDescriptor deviceDescriptor)
        {
            if (!_devices.ContainsKey(deviceDescriptor.DeviceHandle))
            {
                _devices.Add(deviceDescriptor.DeviceHandle, new DiDevice(deviceDescriptor));
            }
        }

        public IDisposable SubscribeBindMode(DeviceDescriptor deviceDescriptor, IObserver<InputModeReport> observer)
        {
            CreateDevice(deviceDescriptor);
            return _devices[deviceDescriptor.DeviceHandle].Subscribe(observer);
        }

        public void SetBindModeState(DeviceDescriptor deviceDescriptor, bool state)
        {
            CreateDevice(deviceDescriptor);
            _devices[deviceDescriptor.DeviceHandle].SetBindModeState(state);
        }
    }
}
