using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;

namespace XInput
{
    public class XiProvider : IProvider
    {
        private readonly Dictionary<DeviceDescriptor, XiDevice> _devices = new Dictionary<DeviceDescriptor, XiDevice>();
        private readonly XiReportHandler _xiReportHandler = new XiReportHandler();

        public XiProvider()
        {
            
        }

        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputReport> observer)
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
                _devices.Add(deviceDescriptor, new XiDevice(deviceDescriptor, DeviceEmptyEventHandler));
            }
        }

        private void DeviceEmptyEventHandler(object sender, DeviceEmptyEventArgs deviceEmptyEventArgs)
        {
            _devices[deviceEmptyEventArgs.DeviceDescriptor].Dispose();
            _devices.Remove(deviceEmptyEventArgs.DeviceDescriptor);
        }

        public IDisposable SubscribeBindMode(DeviceDescriptor deviceDescriptor, IObserver<InputReport> observer)
        {
            CreateDevice(deviceDescriptor);
            return _devices[deviceDescriptor].Subscribe(observer);
        }

        public ProviderReport GetInputList()
        {
            return _xiReportHandler.GetInputList();
        }
    }
}
