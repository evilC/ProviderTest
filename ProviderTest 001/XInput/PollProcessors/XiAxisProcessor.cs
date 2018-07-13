using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common;
using SharpDX.XInput;

namespace XInput.PollProcessors
{
    public class XiAxisProcessor : IPollProcessor<State>, IObservableInput<InputReport>
    {
        public EventHandler<InputReportEventArgs> OnBindMode;
        private readonly EventHandler _observerListEmptyEventHandler;
        private readonly InputDescriptor _inputDescriptor;
        private readonly List<IObserver<InputReport>> _observers = new List<IObserver<InputReport>>();
        
        private static readonly List<string> AxisNames = new List<string>{ "LeftThumbX", "LeftThumbY", "RightThumbX", "RightThumbY", "LeftTrigger", "RightTrigger" };
        private readonly string _axisName;
        private int _lastValue;

        public XiAxisProcessor(InputDescriptor inputDescriptor, EventHandler observerListEmptyEventHandler, EventHandler<InputReportEventArgs> bindModeHandler)
        {
            _observerListEmptyEventHandler = observerListEmptyEventHandler;
            OnBindMode += bindModeHandler;
            _inputDescriptor = inputDescriptor;
            var index = inputDescriptor.BindingDescriptor.Type == BindingType.Button
                ? inputDescriptor.BindingDescriptor.Index
                : inputDescriptor.BindingDescriptor.SubIndex + 10;
            _axisName = AxisNames[inputDescriptor.BindingDescriptor.Index];
        }

        public int GetObserverCount()
        {
            return _observers.Count;
        }

        public IDisposable Subscribe(InputDescriptor subReq, IObserver<InputReport> observer)
        {
            return Subscribe(observer);
        }

        public void ProcessSubscriptionMode(State thisState)
        {
            ProcessPoll(PollMode.Subscription, thisState);
        }

        public void ProcessBindMode(State thisState)
        {
            ProcessPoll(PollMode.Bind, thisState);
        }

        public void ProcessPoll(PollMode mode, State thisState)
        {
            var thisValue = GetValue(thisState);
            if (thisValue == _lastValue) return;
            var report = BuildReport(thisState);

            switch (mode)
            {
                case PollMode.Bind:
                    OnBindMode(this, new InputReportEventArgs(report));
                    break;
                case PollMode.Subscription:
                    foreach (var observer in _observers)
                    {
                        observer.OnNext(report);
                    }
                    break;
                default:
                    throw new Exception($"Unknown Poll Mode: {mode}");
            }

            _lastValue = thisValue;

        }

        private int GetValue(State thisState)
        {
            var value = Convert.ToInt32(thisState.Gamepad.GetType().GetField(_axisName).GetValue(thisState.Gamepad));
            if (_inputDescriptor.BindingDescriptor.Index > 3)
            {
                // Trigger axis
                value = (value * 257) - 32767;
            }
            return value;
        }

        private InputReport BuildReport(State state)
        {
            return new InputReport(_inputDescriptor, GetValue(state));
        }

        public IDisposable Subscribe(IObserver<InputReport> observer)
        {
            _observers.Add(observer);
            return new ObservableUnsubscriber<InputReport>(_observers, observer, _observerListEmptyEventHandler);
        }

    }
}
