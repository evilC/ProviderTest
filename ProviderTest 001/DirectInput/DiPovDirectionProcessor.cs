using System;
using System.Collections.Generic;
using Common;
using SharpDX.DirectInput;

namespace DirectInput
{
    class DiPovDirectionProcessor :IPollProcessor<JoystickUpdate>, IObservableInput<InputModeReport>
    {
        private readonly InputDescriptor _inputDescriptor;
        private readonly int _thisAngle;
        private int _lastState;
        private readonly List<IObserver<InputModeReport>> _observers = new List<IObserver<InputModeReport>>();
        public EventHandler<InputReportEventArgs> OnBindMode;

        public DiPovDirectionProcessor(InputDescriptor inputDescriptor, EventHandler<InputReportEventArgs> bindModeHandler)
        {
            _inputDescriptor = inputDescriptor;
            _thisAngle = _inputDescriptor.BindingDescriptor.SubIndex * 9000;
            OnBindMode += bindModeHandler;
        }

        public IDisposable Subscribe(IObserver<InputModeReport> observer)
        {
            _observers.Add(observer);
            return new ObservableUnsubscriber<InputModeReport>(_observers, observer);
        }

        public IDisposable Subscribe(InputDescriptor subReq, IObserver<InputModeReport> observer)
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
                    observer.OnNext(new InputModeReport(_inputDescriptor, newDirectionState));
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
                OnBindMode(this, new InputReportEventArgs(new InputModeReport(_inputDescriptor, newDirectionState)));
                _lastState = newDirectionState;
            }
        }

        public int GetObserverCount()
        {
            return _observers.Count;
        }
    }
}