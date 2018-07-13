using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using SharpDX.DirectInput;

namespace DirectInput.PollProcessors
{
    public class DiAxisProcessor : IPollProcessor<JoystickUpdate>, IObservableInput<InputReport>
    {
        public EventHandler<InputReportEventArgs> OnBindMode;
        private readonly EventHandler _observerListEmptyEventHandler;

        private readonly InputDescriptor _inputDescriptor;
        private readonly List<IObserver<InputReport>> _observers = new List<IObserver<InputReport>>();

        public DiAxisProcessor(InputDescriptor inputDescriptor, EventHandler observerListEmptyEventHandler, EventHandler<InputReportEventArgs> bindModeHandler)
        {
            _observerListEmptyEventHandler = observerListEmptyEventHandler;
            _inputDescriptor = inputDescriptor;
            OnBindMode += bindModeHandler;
        }

        public int GetObserverCount()
        {
            return _observers.Count();
        }

        public IDisposable Subscribe(InputDescriptor subReq, IObserver<InputReport> observer)
        {
            return Subscribe(observer);
        }

        public void ProcessSubscriptionMode(JoystickUpdate state)
        {
            var report = BuildReport(state);
            foreach (var observer in _observers)
            {
                observer.OnNext(report);
            }
        }

        public void ProcessBindMode(JoystickUpdate state)
        {
            var report = BuildReport(state);
            OnBindMode(this, new InputReportEventArgs(report));
        }

        private InputReport BuildReport(JoystickUpdate state)
        {
            return new InputReport(_inputDescriptor, state.Value - 32768);
        }

        public IDisposable Subscribe(IObserver<InputReport> observer)
        {
            _observers.Add(observer);
            return new ObservableUnsubscriber<InputReport>(_observers, observer, _observerListEmptyEventHandler);
        }
    }
}
