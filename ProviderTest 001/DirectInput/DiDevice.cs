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
    class DiDevice : IObservable<InputModeReport>, IDevice
    {
        private readonly Joystick _device;
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly Dictionary<(BindingType, int), IPollProcessor<JoystickUpdate>> _pollProcessors = new Dictionary<(BindingType, int), IPollProcessor<JoystickUpdate>>();
        private readonly List<IObserver<InputModeReport>> _bindModeObservers = new List<IObserver<InputModeReport>>();
        private PollMode _pollMode = PollMode.Subscription;
        private readonly Thread _pollThread;

        public EventHandler<DeviceEmptyEventArgs> OnDeviceEmpty;

        public DiDevice(DeviceDescriptor deviceDescriptor, EventHandler<DeviceEmptyEventArgs> deviceEmptyEventHandler)
        {
            _deviceDescriptor = deviceDescriptor;
            OnDeviceEmpty = deviceEmptyEventHandler;
            BuildPollProcessors();

            var guid = DiWrapper.Instance.ConnectedDevices[deviceDescriptor.DeviceHandle][deviceDescriptor.DeviceInstance];
            _device = new Joystick(DiWrapper.DiInstance, guid);
            _device.Properties.BufferSize = 128;
            _device.Acquire();

            _pollThread = new Thread(PollThread);
            _pollThread.Start();
        }

        #region Interfaces
        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputModeReport> observer)
        {
            return _pollProcessors[(subReq.BindingDescriptor.Type, subReq.BindingDescriptor.Index)].Subscribe(subReq, observer);
        }

        #region IObservable

        // Bind Mode subscribe
        public IDisposable Subscribe(IObserver<InputModeReport> observer)
        {
            _bindModeObservers.Add(observer);
            return new ObservableUnsubscriber<InputModeReport>(_bindModeObservers, observer, OnBindModeEmpty);
        }

        // Fired when the last subscriber unsubsribes in Bind Mode
        private void OnBindModeEmpty(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _pollThread.Abort();
            _pollThread.Join();
            _device?.Dispose();
        }

        #endregion

        #endregion


        private void BuildPollProcessors()
        {
            // ToDo: Read Device Caps
            for (var i = 0; i < 128; i++)
            {
                var descriptor = new InputDescriptor(_deviceDescriptor, new BindingDescriptor(BindingType.Button, i));
                _pollProcessors[descriptor.BindingDescriptor.ToShortTuple()] = new DiButtonProcessor(descriptor, InputEmptyEventHandler, OnBindMode);
            }

            for (var i = 0; i < 4; i++)
            {
                var descriptor = new InputDescriptor(_deviceDescriptor, new BindingDescriptor(BindingType.POV, i));
                _pollProcessors[descriptor.BindingDescriptor.ToShortTuple()] = new DiPovProcessor(descriptor, InputEmptyEventHandler, OnBindMode);
            }
        }

        private void InputEmptyEventHandler(object sender, EventArgs eventArgs)
        {
            // An Input has indicated that it has no more subscriptions
            // Check all inputs, and if none have any subscriptions, then this device is unused, and can be disposed...
            // ... UNLESS, there are Bind Mode subscriptions active, in which case do not dispose
            if (_bindModeObservers.Count > 0) return;
            var empty = true;
            foreach (var pollProcessor in _pollProcessors.Values)
            {
                if (pollProcessor.GetObserverCount() == 0) continue;
                empty = false;
                break;
            }
            if (!empty) return;

            OnDeviceEmpty(this, new DeviceEmptyEventArgs(_deviceDescriptor));
        }

        private void OnBindMode(object sender, InputReportEventArgs inputReportEventArgs)
        {
            foreach (var bindModeObserver in _bindModeObservers)
            {
                bindModeObserver.OnNext(inputReportEventArgs.InputModeReport);
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
                        var processorTuple = GetInputProcessorKey(state.Offset);
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
                        var processorTuple = GetInputProcessorKey(state.Offset);
                        if (!_pollProcessors.ContainsKey(processorTuple)    // ToDo: Should not happen in production, throw?
                            || _pollProcessors[processorTuple].GetObserverCount() == 0)
                            continue;

                        _pollProcessors[processorTuple].ProcessSubscriptionMode(state);
                    }
                    Thread.Sleep(10);
                }
            }
        }

        public static (BindingType, int) GetInputProcessorKey(JoystickOffset offset)
        {
            if (offset > JoystickOffset.Buttons127) throw new NotImplementedException(); // force etc not implemented
            if (offset >= JoystickOffset.Buttons0) return (BindingType.Button, offset - JoystickOffset.Buttons0);
            if (offset >= JoystickOffset.PointOfViewControllers0) return (BindingType.POV, (offset - JoystickOffset.PointOfViewControllers0) / 4);
            return (BindingType.Axis, (int)offset / 4);
        }

        public void SetBindModeState(bool state)
        {
            _pollMode = state ? PollMode.Bind : PollMode.Subscription;
        }
    }
}
