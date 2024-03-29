﻿using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.XInput;
using XInput.PollProcessors;

namespace XInput
{
    public class XiDevice : InputDevice<State, (BindingType, int, int)>
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
                while (!_device.IsConnected)
                {
                    Thread.Sleep(500);
                }

                while (_device.IsConnected && PollMode == PollMode.Bind)
                {
                    var thisState = _device.GetState();
                    // Axes
                    for (var i = 0; i < 6; i++)
                    {
                        var axisTuple = (BindingType.Axis, i, 0);
                        PollProcessors[axisTuple].ProcessBindMode(thisState);
                    }

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

                while (_device.IsConnected && PollMode == PollMode.Subscription)
                {
                    var thisState = _device.GetState();
                    // Axes
                    for (var i = 0; i < 6; i++)
                    {
                        var axisTuple = (BindingType.Axis, i, 0);
                        if (PollProcessors[axisTuple].GetObserverCount() == 0) continue;

                        PollProcessors[axisTuple].ProcessSubscriptionMode(thisState);
                    }

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
            // Axes
            for (var i = 0; i < 6; i++)
            {
                var descriptor = new InputDescriptor(DeviceDescriptor, new BindingDescriptor(BindingType.Axis, i));
                PollProcessors[GetPollProcessorKey(descriptor.BindingDescriptor)] = new XiAxisProcessor(descriptor, InputEmptyEventHandler, BindModeEventHandler);
            }

            // Buttons
            for (var i = 0; i < 10; i++)
            {
                var descriptor = new InputDescriptor(DeviceDescriptor, new BindingDescriptor(BindingType.Button, i));
                PollProcessors[GetPollProcessorKey(descriptor.BindingDescriptor)] = new XiButtonProcessor(descriptor, InputEmptyEventHandler, BindModeEventHandler);
            }

            // DPad DIRECTIONS (Handled as buttons)
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
