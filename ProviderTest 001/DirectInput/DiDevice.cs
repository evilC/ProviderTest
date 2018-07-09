using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using SharpDX.DirectInput;

namespace DirectInput
{
    class DiDevice
    {
        private readonly Joystick _device;
        private DeviceDescriptor _deviceDescriptor;
        private readonly Dictionary<(BindingType, int), IPollProcessor<JoystickUpdate>> _pollProcessors = new Dictionary<(BindingType, int), IPollProcessor<JoystickUpdate>>();

        public DiDevice(DeviceDescriptor deviceDescriptor)
        {
            _deviceDescriptor = deviceDescriptor;
            BuildPollProcessors();

            var guid = DiWrapper.Instance.ConnectedDevices[deviceDescriptor.DeviceHandle][deviceDescriptor.DeviceInstance];
            _device = new Joystick(DiWrapper.DiInstance, guid);
            _device.Properties.BufferSize = 128;
            _device.Acquire();

            var pollThread = new Thread(PollThread);
            pollThread.Start();
        }

        private void BuildPollProcessors()
        {
            for (var i = 0; i < 128; i++)
            {
                var descriptor = new InputDescriptor(_deviceDescriptor, new BindingDescriptor(BindingType.Button, i));
                _pollProcessors[descriptor.BindingDescriptor.ToShortTuple()] = new DiButtonProcessor(descriptor);
            }

            for (var i = 0; i < 4; i++)
            {
                var descriptor = new InputDescriptor(_deviceDescriptor, new BindingDescriptor(BindingType.POV, i));
                _pollProcessors[descriptor.BindingDescriptor.ToShortTuple()] = new DiPovProcessor(descriptor);
            }
        }

        private void PollThread()
        {
            while (true)
            {
                var data = _device.GetBufferedData();
                foreach (var state in data)
                {
                    var processorTuple = GetInputProcessorKey(state.Offset);
                    if (!_pollProcessors.ContainsKey(processorTuple)) continue;

                    _pollProcessors[processorTuple].ProcessSubscriptionMode(state);
                }
                Thread.Sleep(10);
            }
        }

        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputModeReport> observer)
        {
            return _pollProcessors[(subReq.BindingDescriptor.Type, subReq.BindingDescriptor.Index)].Subscribe(subReq, observer);
        }

        public static (BindingType, int) GetInputProcessorKey(JoystickOffset offset)
        {
            if (offset > JoystickOffset.Buttons127) throw new NotImplementedException(); // force etc not implemented
            if (offset >= JoystickOffset.Buttons0) return (BindingType.Button, offset - JoystickOffset.Buttons0);
            if (offset >= JoystickOffset.PointOfViewControllers0) return (BindingType.POV, (offset - JoystickOffset.PointOfViewControllers0) / 4);
            return (BindingType.Axis, (int)offset / 4);
        }

    }
}
