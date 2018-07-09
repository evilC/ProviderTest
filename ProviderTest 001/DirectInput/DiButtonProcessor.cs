using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using SharpDX.DirectInput;

namespace DirectInput
{
    class DiButtonProcessor : IPollProcessor<JoystickUpdate>, IObservableInput<InputModeReport>
    {
        private readonly InputDescriptor _inputDescriptor;
        private readonly List<IObserver<InputModeReport>> _observers = new List<IObserver<InputModeReport>>();

        public DiButtonProcessor(InputDescriptor inputDescriptor)
        {
            _inputDescriptor = inputDescriptor;
        }

        public IDisposable Subscribe(InputDescriptor subReq, IObserver<InputModeReport> observer)
        {
            return Subscribe(observer);
        }

        public void ProcessSubscriptionMode(JoystickUpdate state)
        {
            var report = new InputModeReport(_inputDescriptor, state.Value == 128 ? 1 : 0);
            foreach (var observer in _observers)
            {
                observer.OnNext(report);
            }
        }

        public IDisposable Subscribe(IObserver<InputModeReport> observer)
        {
            _observers.Add(observer);
            return new ObservableUnsubscriber<InputModeReport>(_observers, observer);
        }

        public int GetObserverCount()
        {
            return _observers.Count;
        }
    }
}
