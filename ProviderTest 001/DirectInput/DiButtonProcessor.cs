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
        private readonly BindingDescriptor _bindingDescriptor;
        private readonly List<IObserver<InputModeReport>> _observers = new List<IObserver<InputModeReport>>();

        public DiButtonProcessor(BindingDescriptor bindingDescriptor)
        {
            _bindingDescriptor = bindingDescriptor;
        }

        public IDisposable Subscribe(BindingDescriptor bindingDescriptor, IObserver<InputModeReport> observer)
        {
            return Subscribe(observer);
        }

        public void ProcessSubscriptionMode(JoystickUpdate state)
        {
            var report = new InputModeReport(_bindingDescriptor, state.Value == 128 ? 1 : 0);
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
