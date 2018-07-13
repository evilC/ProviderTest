using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.XInput;

namespace XInput
{
    class XiDevice : InputDevice<State, (BindingType, int, int)>
    {
        private readonly Thread _pollThread;

        private readonly Controller _device;

        public XiDevice(DeviceDescriptor deviceDescriptor, EventHandler<DeviceEmptyEventArgs> deviceEmptyEventHandler) : base(deviceDescriptor, deviceEmptyEventHandler)
        {
            _device = new Controller((UserIndex)deviceDescriptor.DeviceInstance);

            BuildPollProcessors();

            _pollThread = new Thread(PollThread);
            _pollThread.Start();
        }

        private void PollThread()
        {
            while (true)
            {
                while (PollMode == PollMode.Bind)
                {
                    var thisState = _device.GetState();

                    // Buttons
                    for (var i = 0; i < 10; i++)
                    {
                        var buttonTuple = (BindingType.Button, i, 0);

                        PollProcessors[buttonTuple].ProcessBindMode(thisState);
                    }

                    // DPad
                    for (var i = 0; i < 4; i++)
                    {
                        var buttonTuple = (BindingType.POV, 0, i);

                        PollProcessors[buttonTuple].ProcessBindMode(thisState);
                    }

                    Thread.Sleep(10);
                }

                while (PollMode == PollMode.Subscription)
                {
                    var thisState = _device.GetState();

                    // Buttons
                    for (var i = 0; i < 10; i++)
                    {
                        var buttonTuple = (BindingType.Button, i, 0);
                        if (PollProcessors[buttonTuple].GetObserverCount() == 0) continue;

                        PollProcessors[buttonTuple].ProcessSubscriptionMode(thisState);
                    }

                    // DPad
                    for (var i = 0; i < 4; i++)
                    {
                        var buttonTuple = (BindingType.POV, 0, i);
                        if (PollProcessors[buttonTuple].GetObserverCount() == 0) continue;

                        PollProcessors[buttonTuple].ProcessSubscriptionMode(thisState);
                    }

                    Thread.Sleep(10);
                }
            }
        }

        public override (BindingType, int, int) GetPollProcessorKey(BindingDescriptor bindingDescriptor)
        {
            return (bindingDescriptor.Type, bindingDescriptor.Index, bindingDescriptor.SubIndex);
        }

        public void BuildPollProcessors()
        {
            for (var i = 0; i < 10; i++)
            {
                var descriptor = new InputDescriptor(DeviceDescriptor, new BindingDescriptor(BindingType.Button, i));
                PollProcessors[GetPollProcessorKey(descriptor.BindingDescriptor)] = new XiButtonProcessor(descriptor, InputEmptyEventHandler, BindModeEventHandler);
            }

            for (var i = 0; i < 4; i++)
            {
                var descriptor = new InputDescriptor(DeviceDescriptor, new BindingDescriptor(BindingType.POV, 0, i));
                PollProcessors[GetPollProcessorKey(descriptor.BindingDescriptor)] = new XiButtonProcessor(descriptor, InputEmptyEventHandler, BindModeEventHandler);
            }
        }

        public override void Dispose()
        {
            _pollThread.Abort();
            _pollThread.Join();
        }
    }
}
