using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public abstract class InputDevice<TPollType, TPollProcessorKey> : IDevice, IObservable<InputModeReport>
    {
        protected PollMode _pollMode = PollMode.Subscription;

        protected readonly DeviceDescriptor _deviceDescriptor;

        protected readonly Dictionary<TPollProcessorKey, IPollProcessor<TPollType>> _pollProcessors = new Dictionary<TPollProcessorKey, IPollProcessor<TPollType>>();
        protected readonly List<IObserver<InputModeReport>> _bindModeObservers = new List<IObserver<InputModeReport>>();

        public EventHandler<DeviceEmptyEventArgs> OnDeviceEmpty;


        protected InputDevice(DeviceDescriptor deviceDescriptor, EventHandler<DeviceEmptyEventArgs> deviceEmptyEventHandler)
        {
            _deviceDescriptor = deviceDescriptor;
            OnDeviceEmpty = deviceEmptyEventHandler;
            BuildPollProcessors();
        }

        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputModeReport> observer)
        {
            return _pollProcessors[GetPollProcessorKey(subReq.BindingDescriptor)].Subscribe(subReq, observer);
        }

        // Bind Mode subscribe
        public IDisposable Subscribe(IObserver<InputModeReport> observer)
        {
            _bindModeObservers.Add(observer);
            return new ObservableUnsubscriber<InputModeReport>(_bindModeObservers, observer, BindModeEmptyEventHandler);
        }

        public abstract TPollProcessorKey GetPollProcessorKey(BindingDescriptor bindingDescriptor);

        public abstract void BuildPollProcessors();

        protected void InputEmptyEventHandler(object sender, EventArgs eventArgs)
        {
            // An Input has indicated that it has no more subscriptions
            // Check all inputs, and if none have any subscriptions, then this device is unused, and can be disposed...
            // ... UNLESS, there are Bind Mode subscriptions active, in which case do not dispose
            if (_bindModeObservers.Count > 0 || DeviceHasSubscriptionObservers()) return;

            OnDeviceEmpty(this, new DeviceEmptyEventArgs(_deviceDescriptor));
        }

        // Fired when the last subscriber unsubsribes in Bind Mode
        protected void BindModeEmptyEventHandler(object sender, EventArgs eventArgs)
        {
            if (DeviceHasSubscriptionObservers()) return;

            OnDeviceEmpty(this, new DeviceEmptyEventArgs(_deviceDescriptor));
        }

        protected void BindModeEventHandler(object sender, InputReportEventArgs inputReportEventArgs)
        {
            foreach (var bindModeObserver in _bindModeObservers)
            {
                bindModeObserver.OnNext(inputReportEventArgs.InputModeReport);
            }
        }

        protected bool DeviceHasSubscriptionObservers()
        {
            foreach (var pollProcessor in _pollProcessors.Values)
            {
                if (pollProcessor.GetObserverCount() == 0) continue;
                return true;
            }

            return false;
        }


        #region IDevice

        public abstract void Dispose();

        public void SetBindModeState(bool state)
        {
            _pollMode = state ? PollMode.Bind : PollMode.Subscription;
        }

        #endregion
    }
}
