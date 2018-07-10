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
        private readonly Dictionary<int, XiDevice> _devices = new Dictionary<int, XiDevice>();

        public XiProvider()
        {
            
        }

        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputModeReport> observer)
        {
            CreateDevice(subReq.DeviceDescriptor);
            return _devices[subReq.DeviceDescriptor.DeviceInstance].SubscribeInput(subReq, observer);
        }

        public void SetBindModeState(DeviceDescriptor deviceDescriptor, bool state)
        {
            CreateDevice(deviceDescriptor);
            _devices[deviceDescriptor.DeviceInstance].SetBindModeState(state);
        }

        private void CreateDevice(DeviceDescriptor deviceDescriptor)
        {
            if (!_devices.ContainsKey(deviceDescriptor.DeviceInstance))
            {
                _devices.Add(deviceDescriptor.DeviceInstance, new XiDevice(deviceDescriptor));
            }
        }

        public IDisposable SubscribeBindMode(DeviceDescriptor deviceDescriptor, IObserver<InputModeReport> observer)
        {
            CreateDevice(deviceDescriptor);
            return _devices[deviceDescriptor.DeviceInstance].Subscribe(observer);
        }
    }
}
