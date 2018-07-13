using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public abstract class InputDevice<TPollType, TPollProcessorKey> : IDevice, IObservable<InputReport>
    {
        // What happens when we poll? Fire Subscriptions for just subscribed inputs, or send all input to the Bind Mode handler?
        protected PollMode PollMode = PollMode.Subscription;

        // Describes this device
        protected readonly DeviceDescriptor DeviceDescriptor;

        // Dictionary of classes that handle processing of poll data
        protected readonly Dictionary<TPollProcessorKey, IPollProcessor<TPollType>> PollProcessors = new Dictionary<TPollProcessorKey, IPollProcessor<TPollType>>();

        // List of observers for Bind Mode
        protected readonly List<IObserver<InputReport>> BindModeObservers = new List<IObserver<InputReport>>();

        // Fired when the device has no subscriptions
        public EventHandler<DeviceEmptyEventArgs> OnDeviceEmpty;

        /// <summary>
        /// Handles a device (eg a Joystick)
        /// </summary>
        /// <param name="deviceDescriptor">The Descriptor which describes this device</param>
        /// <param name="deviceEmptyEventHandler">An Event Handler which is fired when this device has no more Subscriptions</param>
        protected InputDevice(DeviceDescriptor deviceDescriptor, EventHandler<DeviceEmptyEventArgs> deviceEmptyEventHandler)
        {
            DeviceDescriptor = deviceDescriptor;
            OnDeviceEmpty = deviceEmptyEventHandler;
        }

        #region IDevice

        public abstract void Dispose();

        public void SetBindModeState(bool state)
        {
            PollMode = state ? PollMode.Bind : PollMode.Subscription;
        }

        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputReport> observer)
        {
            return PollProcessors[GetPollProcessorKey(subReq.BindingDescriptor)].Subscribe(subReq, observer);
        }

        public IDisposable Subscribe(IObserver<InputReport> observer)
        {
            BindModeObservers.Add(observer);
            return new ObservableUnsubscriber<InputReport>(BindModeObservers, observer, BindModeEmptyEventHandler);
        }

        #endregion

        /// <summary>
        /// Returns the key for PollProcessors dictionary that matches the specified Binding
        /// </summary>
        /// <param name="bindingDescriptor">The descriptor that describes this binding</param>
        /// <returns></returns>
        public abstract TPollProcessorKey GetPollProcessorKey(BindingDescriptor bindingDescriptor);

        /// <summary>
        /// Fired when one of the device's inputs no longer has any Subscriptions
        /// </summary>
        /// <param name="sender">The Input that raised the event</param>
        /// <param name="eventArgs">Not used</param>
        protected void InputEmptyEventHandler(object sender, EventArgs eventArgs)
        {
            // An Input has indicated that it has no more subscriptions
            // Check all inputs, and if none have any subscriptions, then this device is unused, and can be disposed...
            // ... UNLESS, there are Bind Mode subscriptions active, in which case do not dispose
            if (BindModeObservers.Count > 0 || DeviceHasSubscriptionObservers()) return;

            OnDeviceEmpty(this, new DeviceEmptyEventArgs(DeviceDescriptor));
        }

        /// <summary>
        /// Fired when the last subscriber unsubsribes in Bind Mode
        /// </summary>
        /// <param name="sender">The Input that raised the event</param>
        /// <param name="eventArgs">Not used</param>
        protected void BindModeEmptyEventHandler(object sender, EventArgs eventArgs)
        {
            if (DeviceHasSubscriptionObservers()) return;

            OnDeviceEmpty(this, new DeviceEmptyEventArgs(DeviceDescriptor));
        }

        /// <summary>
        /// Fired when one of the inputs received input while in Bind Mode
        /// </summary>
        /// <param name="sender">The input that raised the event</param>
        /// <param name="inputReportEventArgs">The InputDescriptor for the input</param>
        protected void BindModeEventHandler(object sender, InputReportEventArgs inputReportEventArgs)
        {
            foreach (var bindModeObserver in BindModeObservers)
            {
                bindModeObserver.OnNext(inputReportEventArgs.InputReport);
            }
        }

        /// <summary>
        /// Helper method that checks if any of the inputs on this device has Subscriptions
        /// </summary>
        /// <returns>True if this device has subscriptions, else false</returns>
        protected bool DeviceHasSubscriptionObservers()
        {
            foreach (var pollProcessor in PollProcessors.Values)
            {
                if (pollProcessor.GetObserverCount() == 0) continue;
                return true;
            }

            return false;
        }
    }
}
