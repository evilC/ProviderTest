using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (!_devices.ContainsKey(subReq.DeviceDescriptor.DeviceInstance))
            {
                _devices.Add(subReq.DeviceDescriptor.DeviceInstance, new XiDevice(subReq.DeviceDescriptor));
            }
            return _devices[subReq.DeviceDescriptor.DeviceInstance].SubscribeInput(subReq, observer);
        }
    }
}
