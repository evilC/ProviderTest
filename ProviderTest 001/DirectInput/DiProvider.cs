using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace DirectInput
{
    public class DiProvider : IProvider, IDisposable
    {
        private readonly Dictionary<DeviceDescriptor, DiDevice> _devices = new Dictionary<DeviceDescriptor, DiDevice>();
        private readonly DiReportHandler _diReportHandler = new DiReportHandler();
        private readonly Thread _deviceWatcherThread;

        public DiProvider()
        {
            _deviceWatcherThread = new Thread(DeviceWatcherThread);
            _deviceWatcherThread.Start();
        }

        // Used to detect device connect / disconnect
        private void DeviceWatcherThread()
        {
            while (true)
            {
                DiWrapper.Instance.RefreshConnectedDevices();
                foreach (var device in _devices)
                {
                    if (!device.Value.Acquired &&
                        DiWrapper.Instance.ConnectedDevices.ContainsKey(device.Key.DeviceHandle)
                        && DiWrapper.Instance.ConnectedDevices[device.Key.DeviceHandle].Count >= device.Key.DeviceInstance)
                    {
                        device.Value.SetAcquireState(true);
                    }
                }
                Thread.Sleep(500);
            }
        }

        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputReport> observer)
        {
            //if (!DiWrapper.Instance.ConnectedDevices.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
            //{
            //    throw new Exception("Device not found");
            //}

            CreateDevice(subReq.DeviceDescriptor);
            return _devices[subReq.DeviceDescriptor].SubscribeInput(subReq, observer);
        }

        public void CreateDevice(DeviceDescriptor deviceDescriptor)
        {
            if (!_devices.ContainsKey(deviceDescriptor))
            {
                _devices.Add(deviceDescriptor, new DiDevice(deviceDescriptor, DeviceEmptyEventHandler));
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

        public void SetBindModeState(DeviceDescriptor deviceDescriptor, bool state)
        {
            CreateDevice(deviceDescriptor);
            _devices[deviceDescriptor].SetBindModeState(state);
        }

        public void Dispose()
        {
            _deviceWatcherThread.Abort();
            _deviceWatcherThread.Join();
        }

        public ProviderReport GetInputList()
        {
            return _diReportHandler.GetInputList();
        }
    }
}
