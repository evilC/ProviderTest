using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;

namespace XInput
{
    public class XiProvider
    {
        private readonly Dictionary<DeviceDescriptor, XiDevice> _devices = new Dictionary<DeviceDescriptor, XiDevice>();

        public XiProvider()
        {
            
        }

        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputModeReport> observer)
        {
            CreateDevice(subReq.DeviceDescriptor);
            return _devices[subReq.DeviceDescriptor].SubscribeInput(subReq, observer);
        }

        public void SetBindModeState(DeviceDescriptor deviceDescriptor, bool state)
        {
            CreateDevice(deviceDescriptor);
            _devices[deviceDescriptor].SetBindModeState(state);
        }

        private void CreateDevice(DeviceDescriptor deviceDescriptor)
        {
            if (!_devices.ContainsKey(deviceDescriptor))
            {
                _devices.Add(deviceDescriptor, new XiDevice(deviceDescriptor));
            }
        }

        public IDisposable SubscribeBindMode(DeviceDescriptor deviceDescriptor, IObserver<InputModeReport> observer)
        {
            CreateDevice(deviceDescriptor);
            return _devices[deviceDescriptor].Subscribe(observer);
        }
    }
}
