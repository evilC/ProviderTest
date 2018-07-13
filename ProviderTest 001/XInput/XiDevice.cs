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

            _pollThread = new Thread(PollThread);
            _pollThread.Start();
        }

        private void PollThread()
        {
            while (true)
            {
                while (_pollMode == PollMode.Bind)
                {
                    var thisState = _device.GetState();

                    // Buttons
                    for (var i = 0; i < 10; i++)
                    {
                        var buttonTuple = (BindingType.Button, i, 0);

                        _pollProcessors[buttonTuple].ProcessBindMode(thisState);
                    }

                    // DPad
                    for (var i = 0; i < 4; i++)
                    {
                        var buttonTuple = (BindingType.POV, 0, i);

                        _pollProcessors[buttonTuple].ProcessBindMode(thisState);
                    }

                    Thread.Sleep(10);
                }

                while (_pollMode == PollMode.Subscription)
                {
                    var thisState = _device.GetState();

                    // Buttons
                    for (var i = 0; i < 10; i++)
                    {
                        var buttonTuple = (BindingType.Button, i, 0);
                        if (_pollProcessors[buttonTuple].GetObserverCount() == 0) continue;

                        _pollProcessors[buttonTuple].ProcessSubscriptionMode(thisState);
                    }

                    // DPad
                    for (var i = 0; i < 4; i++)
                    {
                        var buttonTuple = (BindingType.POV, 0, i);
                        if (_pollProcessors[buttonTuple].GetObserverCount() == 0) continue;

                        _pollProcessors[buttonTuple].ProcessSubscriptionMode(thisState);
                    }

                    Thread.Sleep(10);
                }
            }
        }

        public override (BindingType, int, int) GetPollProcessorKey(BindingDescriptor bindingDescriptor)
        {
            return (bindingDescriptor.Type, bindingDescriptor.Index, bindingDescriptor.SubIndex);
        }

        public override void BuildPollProcessors()
        {
            for (var i = 0; i < 10; i++)
            {
                var descriptor = new InputDescriptor(_deviceDescriptor, new BindingDescriptor(BindingType.Button, i));
                _pollProcessors[GetPollProcessorKey(descriptor.BindingDescriptor)] = new XiButtonProcessor(descriptor, InputEmptyEventHandler, BindModeEventHandler);
            }

            for (var i = 0; i < 4; i++)
            {
                var descriptor = new InputDescriptor(_deviceDescriptor, new BindingDescriptor(BindingType.POV, 0, i));
                _pollProcessors[GetPollProcessorKey(descriptor.BindingDescriptor)] = new XiButtonProcessor(descriptor, InputEmptyEventHandler, BindModeEventHandler);
            }
        }

        public override void Dispose()
        {
            _pollThread.Abort();
            _pollThread.Join();
        }
    }
}
