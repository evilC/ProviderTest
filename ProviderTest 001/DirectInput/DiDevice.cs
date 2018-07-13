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
    class DiDevice : InputDevice<JoystickUpdate, (BindingType, int)>
    {
        private readonly Joystick _device;
        private readonly Thread _pollThread;

        public DiDevice(DeviceDescriptor deviceDescriptor, EventHandler<DeviceEmptyEventArgs> deviceEmptyEventHandler) : base(deviceDescriptor, deviceEmptyEventHandler)
        {
            var guid = DiWrapper.Instance.ConnectedDevices[deviceDescriptor.DeviceHandle][deviceDescriptor.DeviceInstance];
            _device = new Joystick(DiWrapper.DiInstance, guid);
            _device.Properties.BufferSize = 128;
            _device.Acquire();

            _pollThread = new Thread(PollThread);
            _pollThread.Start();
        }

        #region Interfaces

        #region IObservable

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            _pollThread.Abort();
            _pollThread.Join();
            _device?.Dispose();
        }

        #endregion

        #endregion


        public override void BuildPollProcessors()
        {
            // ToDo: Read Device Caps
            for (var i = 0; i < 128; i++)
            {
                var descriptor = new InputDescriptor(_deviceDescriptor, new BindingDescriptor(BindingType.Button, i));
                _pollProcessors[GetPollProcessorKey(descriptor.BindingDescriptor)] = new DiButtonProcessor(descriptor, InputEmptyEventHandler, BindModeEventHandler);
            }

            for (var i = 0; i < 4; i++)
            {
                var descriptor = new InputDescriptor(_deviceDescriptor, new BindingDescriptor(BindingType.POV, i));
                _pollProcessors[GetPollProcessorKey(descriptor.BindingDescriptor)] = new DiPovProcessor(descriptor, InputEmptyEventHandler, BindModeEventHandler);
            }
        }

        private void PollThread()
        {
            while (true)
            {
                while (_pollMode == PollMode.Bind)
                {
                    var data = _device.GetBufferedData();
                    foreach (var state in data)
                    {
                        var processorTuple = GetPollProcessorKey(state.Offset);
                        if (!_pollProcessors.ContainsKey(processorTuple)) continue; // ToDo: Handle properly

                        _pollProcessors[processorTuple].ProcessBindMode(state);
                    }
                    Thread.Sleep(10);
                }

                while (_pollMode == PollMode.Subscription)
                {
                    var data = _device.GetBufferedData();
                    foreach (var state in data)
                    {
                        var processorTuple = GetPollProcessorKey(state.Offset);
                        if (!_pollProcessors.ContainsKey(processorTuple)    // ToDo: Should not happen in production, throw?
                            || _pollProcessors[processorTuple].GetObserverCount() == 0)
                            continue;

                        _pollProcessors[processorTuple].ProcessSubscriptionMode(state);
                    }
                    Thread.Sleep(10);
                }
            }
        }

        public static (BindingType, int) GetPollProcessorKey(JoystickOffset offset)
        {
            if (offset > JoystickOffset.Buttons127) throw new NotImplementedException(); // force etc not implemented
            if (offset >= JoystickOffset.Buttons0) return (BindingType.Button, offset - JoystickOffset.Buttons0);
            if (offset >= JoystickOffset.PointOfViewControllers0) return (BindingType.POV, (offset - JoystickOffset.PointOfViewControllers0) / 4);
            return (BindingType.Axis, (int)offset / 4);
        }

        public override (BindingType, int) GetPollProcessorKey(BindingDescriptor bindingDescriptor)
        {
            return (bindingDescriptor.Type, bindingDescriptor.Index);
        }

    }
}
