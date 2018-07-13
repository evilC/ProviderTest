using System;
using System.Collections.Generic;

namespace Common
{
    public class ObservableUnsubscriber<TObservableType> : IDisposable
    {
        public EventHandler OnObserverListEmpty;
        private readonly List<IObserver<TObservableType>> _observers;
        private readonly IObserver<TObservableType> _observer;

        public ObservableUnsubscriber(List<IObserver<TObservableType>> observers, IObserver<TObservableType> observer, EventHandler onObserverListEmpty)
        {
            OnObserverListEmpty = onObserverListEmpty;
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
            if (_observers.Count == 0)
            {
                OnObserverListEmpty(this, EventArgs.Empty);
            }
        }
    }
}