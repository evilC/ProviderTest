using System;
using System.Collections.Generic;
using System.Threading;
using Common;
using SharpDX.DirectInput;

namespace DirectInput.PollProcessors
{
    public class DiPovDirectionProcessor :IPollProcessor<JoystickUpdate>, IObservableInput<InputReport>
    {
        private readonly InputDescriptor _inputDescriptor;
        private readonly int _thisAngle;
        private int _lastState;
        private readonly List<IObserver<InputReport>> _observers = new List<IObserver<InputReport>>();
        public EventHandler<InputReportEventArgs> OnBindMode;
        private readonly EventHandler _observerListEmptyEventHandler;

        public DiPovDirectionProcessor(InputDescriptor inputDescriptor, EventHandler observerListEmptyEventHandler, EventHandler<InputReportEventArgs> bindModeHandler)
        {
            _observerListEmptyEventHandler = observerListEmptyEventHandler;
            _inputDescriptor = inputDescriptor;
            _thisAngle = _inputDescriptor.BindingDescriptor.SubIndex * 9000;
            OnBindMode += bindModeHandler;
        }

        public IDisposable Subscribe(IObserver<InputReport> observer)
        {
            _observers.Add(observer);
            return new ObservableUnsubscriber<InputReport>(_observers, observer, _observerListEmptyEventHandler);
        }

        public IDisposable Subscribe(InputDescriptor subReq, IObserver<InputReport> observer)
        {
            return Subscribe(observer);
        }

        public void ProcessSubscriptionMode(JoystickUpdate state)
        {
            var newAngle = state.Value;
            var newDirectionState =
                newAngle == -1
                    ? 0
                    : PovHelper.StateFromAngle(newAngle, _thisAngle);
            if (newDirectionState != _lastState)
            {
                foreach (var observer in _observers)
                {
                    ThreadPool.QueueUserWorkItem(threadProc => observer.OnNext(new InputReport(_inputDescriptor, newDirectionState)));
                }
                _lastState = newDirectionState;
            }
        }

        public void ProcessBindMode(JoystickUpdate state)
        {
            var newAngle = state.Value;
            var newDirectionState =
                newAngle == -1
                    ? 0
                    : PovHelper.StateFromAngle(newAngle, _thisAngle);
            if (newDirectionState != _lastState)
            {
                OnBindMode(this, new InputReportEventArgs(new InputReport(_inputDescriptor, newDirectionState)));
                _lastState = newDirectionState;
            }
        }

        public int GetObserverCount()
        {
            return _observers.Count;
        }
    }
}