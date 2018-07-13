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
    class XiDevice : IObservable<InputModeReport>, IDevice
    {
        private readonly Dictionary<(BindingType, int, int), IPollProcessor<State>> _pollProcessors = new Dictionary<(BindingType, int, int), IPollProcessor<State>>();
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly List<IObserver<InputModeReport>> _bindModeObservers = new List<IObserver<InputModeReport>>();

        private PollMode _pollMode = PollMode.Subscription;
        private readonly Thread _pollThread;
        public EventHandler<DeviceEmptyEventArgs> OnDeviceEmpty;

        private readonly Controller _device;

        public XiDevice(DeviceDescriptor deviceDescriptor, EventHandler<DeviceEmptyEventArgs> deviceEmptyEventHandler)
        {
            OnDeviceEmpty = deviceEmptyEventHandler;
            _device = new Controller((UserIndex)deviceDescriptor.DeviceInstance);

            BuildPollProcessors();
            _deviceDescriptor = deviceDescriptor;

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
                        var buttonTuple = BuildTuple(BindingType.Button, i);

                        _pollProcessors[buttonTuple].ProcessBindMode(thisState);
                    }

                    // DPad
                    for (var i = 0; i < 4; i++)
                    {
                        var buttonTuple = BuildTuple(BindingType.POV, 0, i);

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
                        var buttonTuple = BuildTuple(BindingType.Button, i);
                        if (_pollProcessors[buttonTuple].GetObserverCount() == 0) continue;

                        _pollProcessors[buttonTuple].ProcessSubscriptionMode(thisState);
                    }

                    // DPad
                    for (var i = 0; i < 4; i++)
                    {
                        var buttonTuple = BuildTuple(BindingType.POV, 0, i);
                        if (_pollProcessors[buttonTuple].GetObserverCount() == 0) continue;

                        _pollProcessors[buttonTuple].ProcessSubscriptionMode(thisState);
                    }

                    Thread.Sleep(10);
                }
            }
        }

        public (BindingType, int, int) BuildTuple(BindingType bindingType, int index, int subIndex = 0)
        {
            return (bindingType, index, subIndex);
        }


        private void BuildPollProcessors()
        {
            for (var i = 0; i < 10; i++)
            {
                var descriptor = new InputDescriptor(_deviceDescriptor, new BindingDescriptor(BindingType.Button, i));
                _pollProcessors[descriptor.BindingDescriptor.ToTuple()] = new XiButtonProcessor(descriptor, InputEmptyEventHandler, OnBindMode);
            }

            for (var i = 0; i < 4; i++)
            {
                var descriptor = new InputDescriptor(_deviceDescriptor, new BindingDescriptor(BindingType.POV, 0, i));
                _pollProcessors[descriptor.BindingDescriptor.ToTuple()] = new XiButtonProcessor(descriptor, InputEmptyEventHandler, OnBindMode);
            }
        }

        private void InputEmptyEventHandler(object sender, EventArgs eventArgs)
        {
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

        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputModeReport> observer)
        {
            var tuple = subReq.BindingDescriptor.ToTuple();
            return _pollProcessors[tuple].Subscribe(subReq, observer);
        }

        private void OnBindMode(object sender, InputReportEventArgs inputReportEventArgs)
        {
            foreach (var bindModeObserver in _bindModeObservers)
            {
                bindModeObserver.OnNext(inputReportEventArgs.InputModeReport);
            }
        }

        // Bind Mode subscribe
        public IDisposable Subscribe(IObserver<InputModeReport> observer)
        {
            _bindModeObservers.Add(observer);
            return new ObservableUnsubscriber<InputModeReport>(_bindModeObservers, observer, OnBindModeEmpty);
        }

        private void OnBindModeEmpty(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public void SetBindModeState(bool state)
        {
            _pollMode = state ? PollMode.Bind : PollMode.Subscription;
        }

        public void Dispose()
        {
            _pollThread.Abort();
            _pollThread.Join();
        }
    }
}
