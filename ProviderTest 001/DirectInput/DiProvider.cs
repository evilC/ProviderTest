using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace DirectInput
{
    public class DiProvider : IDisposable
    {
        private readonly Dictionary<DeviceDescriptor, DiDevice> _devices = new Dictionary<DeviceDescriptor, DiDevice>();

        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputModeReport> observer)
        {
            if (!DiWrapper.Instance.ConnectedDevices.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
            {
                throw new Exception("Device not found");
            }

            CreateDevice(subReq.DeviceDescriptor);
            return _devices[subReq.DeviceDescriptor].SubscribeInput(subReq, observer);
        }

        public void CreateDevice(DeviceDescriptor deviceDescriptor)
        {
            if (!_devices.ContainsKey(deviceDescriptor))
            {
                _devices.Add(deviceDescriptor, new DiDevice(deviceDescriptor));
            }
        }

        public IDisposable SubscribeBindMode(DeviceDescriptor deviceDescriptor, IObserver<InputModeReport> observer)
        {
            CreateDevice(deviceDescriptor);
            return _devices[deviceDescriptor].Subscribe(observer);
        }

        public void SetBindModeState(DeviceDescriptor deviceDescriptor, bool state)
        {
            CreateDevice(deviceDescriptor);
            _devices[deviceDescriptor].SetBindModeState(state);
        }

        public void Dispose()
        {
        }
    }
}
