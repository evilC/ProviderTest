using System;
using System.Collections.Generic;
using Common;
using SharpDX.DirectInput;

namespace DirectInput
{
    class DiPovDirectionProcessor : IObservableInput<InputModeReport>
    {
        private readonly BindingDescriptor _bindingDescriptor;
        private readonly int _thisAngle;
        private int _lastState;
        private readonly List<IObserver<InputModeReport>> _observers = new List<IObserver<InputModeReport>>();

        public DiPovDirectionProcessor(BindingDescriptor bindingDescriptor)
        {
            _bindingDescriptor = bindingDescriptor;
            _thisAngle = _bindingDescriptor.SubIndex * 9000;
        }

        public IDisposable Subscribe(IObserver<InputModeReport> observer)
        {
            _observers.Add(observer);
            return new ObservableUnsubscriber<InputModeReport>(_observers, observer);
        }

        public IDisposable Subscribe(BindingDescriptor bindingDescriptor, IObserver<InputModeReport> observer)
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
                    observer.OnNext(new InputModeReport(_bindingDescriptor, newDirectionState));
                }
                _lastState = newDirectionState;
            }
        }

        public int GetObserverCount()
        {
            return _observers.Count;
        }
    }
}