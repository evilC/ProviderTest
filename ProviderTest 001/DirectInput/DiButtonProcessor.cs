﻿using System;
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
        public EventHandler<InputReportEventArgs> OnBindMode;
        private readonly EventHandler _observerListEmptyEventHandler;

        private readonly InputDescriptor _inputDescriptor;
        private readonly List<IObserver<InputModeReport>> _observers = new List<IObserver<InputModeReport>>();

        public DiButtonProcessor(InputDescriptor inputDescriptor, EventHandler observerListEmptyEventHandler , EventHandler<InputReportEventArgs> bindModeHandler)
        {
            _observerListEmptyEventHandler = observerListEmptyEventHandler;
            _inputDescriptor = inputDescriptor;
            OnBindMode += bindModeHandler;
        }

        public IDisposable Subscribe(InputDescriptor subReq, IObserver<InputModeReport> observer)
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

        private InputModeReport BuildReport(JoystickUpdate state)
        {
            return new InputModeReport(_inputDescriptor, state.Value == 128 ? 1 : 0);
        }

        public IDisposable Subscribe(IObserver<InputModeReport> observer)
        {
            _observers.Add(observer);
            return new ObservableUnsubscriber<InputModeReport>(_observers, observer, _observerListEmptyEventHandler);
        }

        public int GetObserverCount()
        {
            return _observers.Count;
        }
    }
}
