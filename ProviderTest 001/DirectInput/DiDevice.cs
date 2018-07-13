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
    public class DiDevice : InputDevice<JoystickUpdate, (BindingType, int)>
    {
        private readonly Joystick _device;
        private readonly Thread _pollThread;

        public DiDevice(DeviceDescriptor deviceDescriptor, EventHandler<DeviceEmptyEventArgs> deviceEmptyEventHandler) : base(deviceDescriptor, deviceEmptyEventHandler)
        {
            var guid = DiWrapper.Instance.ConnectedDevices[deviceDescriptor.DeviceHandle][deviceDescriptor.DeviceInstance];
            _device = new Joystick(DiWrapper.DiInstance, guid);
            _device.Properties.BufferSize = 128;
            _device.Acquire();

            BuildPollProcessors();

            _pollThread = new Thread(PollThread);
            _pollThread.Start();
        }

        public override void Dispose()
        {
            _pollThread.Abort();
            _pollThread.Join();
            _device?.Dispose();
        }

        public void BuildPollProcessors()
        {
            // ToDo: Read Device Caps
            for (var i = 0; i < 128; i++)
            {
                var descriptor = new InputDescriptor(DeviceDescriptor, new BindingDescriptor(BindingType.Button, i));
                PollProcessors[GetPollProcessorKey(descriptor.BindingDescriptor)] = new DiButtonProcessor(descriptor, InputEmptyEventHandler, BindModeEventHandler);
            }

            for (var i = 0; i < 4; i++)
            {
                var descriptor = new InputDescriptor(DeviceDescriptor, new BindingDescriptor(BindingType.POV, i));
                PollProcessors[GetPollProcessorKey(descriptor.BindingDescriptor)] = new DiPovProcessor(descriptor, InputEmptyEventHandler, BindModeEventHandler);
            }
        }

        private void PollThread()
        {
            while (true)
            {
                while (PollMode == PollMode.Bind)
                {
                    var data = _device.GetBufferedData();
                    foreach (var state in data)
                    {
                        var processorTuple = GetPollProcessorKey(state.Offset);
                        if (!PollProcessors.ContainsKey(processorTuple)) continue; // ToDo: Handle properly

                        PollProcessors[processorTuple].ProcessBindMode(state);
                    }
                    Thread.Sleep(10);
                }

                while (PollMode == PollMode.Subscription)
                {
                    var data = _device.GetBufferedData();
                    foreach (var state in data)
                    {
                        var processorTuple = GetPollProcessorKey(state.Offset);
                        if (!PollProcessors.ContainsKey(processorTuple)    // ToDo: Should not happen in production, throw?
                            || PollProcessors[processorTuple].GetObserverCount() == 0)
                            continue;

                        PollProcessors[processorTuple].ProcessSubscriptionMode(state);
                    }
                    Thread.Sleep(10);
                }
            }
        }

        public static (BindingType, int) GetPollProcessorKey(JoystickOffset offset)
        {
            if (offset > JoystickOffset.Buttons127) throw new NotImplementedException(); // force etc not implemented
            if (offset >= JoystickOffset.Buttons0) return (BindingType.Button, offset - JoystickOffset.Buttons0);
            return offset >= JoystickOffset.PointOfViewControllers0
                ? (BindingType.POV, (offset - JoystickOffset.PointOfViewControllers0) / 4) 
                : (BindingType.Axis, (int)offset / 4);
        }

        public override (BindingType, int) GetPollProcessorKey(BindingDescriptor bindingDescriptor)
        {
            return (bindingDescriptor.Type, bindingDescriptor.Index);
        }

    }
}
