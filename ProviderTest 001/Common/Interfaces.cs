using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// IPollProcessor classes can process (or route) Subsription Requests / Poll data
    /// The class does not need to handle Subscription or poll itself, it may just forward the request on to another class
    /// <typeparam name="TPollType">The type of the report</typeparam>
    /// </summary>
    public interface IPollProcessor<TPollType> : IObservableBase
    {
        IDisposable Subscribe(InputDescriptor subReq, IObserver<InputModeReport> observer);
        void ProcessSubscriptionMode(TPollType state);
        void ProcessBindMode(TPollType state);
    }

    /// <summary>
    /// Extends IObservable, mainly to allow polling classes to check if Observable classes have any subscriptions
    /// This is used to make polling efficient by only processing inputs which have subscribers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObservableInput<T> : IObservableBase, IObservable<T>
    {
        
    }

    public interface IObservableBase
    {
        int GetObserverCount();
    }
}
