using System;
using System.Collections.Generic;

namespace Common
{
    public class ObservableUnsubscriber<TObservableType> : IDisposable
    {
        private readonly List<IObserver<TObservableType>> _observers;
        private readonly IObserver<TObservableType> _observer;

        public ObservableUnsubscriber(List<IObserver<TObservableType>> observers, IObserver<TObservableType> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}