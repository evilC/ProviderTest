using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using DirectInput.PollProcessors;
using SharpDX;
using SharpDX.DirectInput;

namespace DirectInput
{
    public class DiDevice : InputDevice<JoystickUpdate, (BindingType, int)>
    {
        private Joystick _device;
        private Thread _pollThread;

        private bool _pollThreadRunning = false;

        public DiDevice(DeviceDescriptor deviceDescriptor, EventHandler<DeviceEmptyEventArgs> deviceEmptyEventHandler) : base(deviceDescriptor, deviceEmptyEventHandler)
        {
            BuildPollProcessors();
            SetAcquireState(true);
        }

        public bool Acquired { get; private set; } = false;

        public bool SetAcquireState(bool state)
        {
            if (Acquired == state) return true;
            if (state)
            {
                if (!DiWrapper.Instance.ConnectedDevices.ContainsKey(DeviceDescriptor.DeviceHandle) || DiWrapper.Instance.ConnectedDevices[DeviceDescriptor.DeviceHandle].Count < DeviceDescriptor.DeviceInstance)
                {
                    return false;
                }

                try
                {
                    var guid =
                        DiWrapper.Instance.ConnectedDevices[DeviceDescriptor.DeviceHandle][
                            DeviceDescriptor.DeviceInstance];
                    _device = new Joystick(DiWrapper.DiInstance, guid);
                    _device.Properties.BufferSize = 128;
                    _device.Acquire();
                    Acquired = true;
                    Console.WriteLine($"Acquired Device {DeviceDescriptor.DeviceHandle}, Instance {DeviceDescriptor.DeviceInstance}");
                    SetPollThreadState(true);
                    return true;
                }
                catch
                {
                    _device?.Dispose();
                }
            }
            else
            {
                _device?.Dispose();
            }
            Console.WriteLine($"Relinquished Device {DeviceDescriptor.DeviceHandle}, Instance {DeviceDescriptor.DeviceInstance}");
            Acquired = false;
            SetPollThreadState(false);
            return false;
        }

        private void SetPollThreadState(bool state)
        {
            if (state == _pollThreadRunning)
            {
                Console.WriteLine($"Taking no action as Pollthread is already in state {state}");
                return;
            };
            Console.WriteLine($"{(state ? "Starting" : "Stopping")} Di PollThread for Device {DeviceDescriptor.DeviceHandle}, Instance {DeviceDescriptor.DeviceInstance}");
            if (state)
            {
                _pollThread = new Thread(PollThread);
                _pollThread.Start();
                _pollThreadRunning = true;
            }
            else
            {
                _pollThreadRunning = false;
                _pollThread.Abort();
                _pollThread.Join();
                _pollThread = null;
            }
        }

        public override void Dispose()
        {
            //SetPollThreadState(false);
            SetAcquireState(false);
            //_device?.Dispose();
        }

        public void BuildPollProcessors()
        {
            // ToDo: Read Device Caps
            // Axes
            for (var i = 0; i < 8; i++)
            {
                var descriptor = new InputDescriptor(DeviceDescriptor, new BindingDescriptor(BindingType.Axis, i));
                PollProcessors[GetPollProcessorKey(descriptor.BindingDescriptor)] = new DiAxisProcessor(descriptor, InputEmptyEventHandler, BindModeEventHandler);
            }

            // Buttons
            for (var i = 0; i < 128; i++)
            {
                var descriptor = new InputDescriptor(DeviceDescriptor, new BindingDescriptor(BindingType.Button, i));
                PollProcessors[GetPollProcessorKey(descriptor.BindingDescriptor)] = new DiButtonProcessor(descriptor, InputEmptyEventHandler, BindModeEventHandler);
            }

            // POVs
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
                try
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
                            if (!PollProcessors
                                    .ContainsKey(processorTuple) // ToDo: Should not happen in production, throw?
                                || PollProcessors[processorTuple].GetObserverCount() == 0)
                                continue;

                            PollProcessors[processorTuple].ProcessSubscriptionMode(state);
                        }

                        Thread.Sleep(10);
                    }
                }
                catch (SharpDXException ex) when(ex.Descriptor.ApiCode == "InputLost")
                {
                    Console.WriteLine($"Connection Lost for Device {DeviceDescriptor.DeviceHandle}, Instance {DeviceDescriptor.DeviceInstance}");
                    SetAcquireState(false);
                    break;
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
