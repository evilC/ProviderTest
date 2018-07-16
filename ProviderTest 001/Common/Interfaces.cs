using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IProvider
    {
        ProviderReport GetInputList();
    }

    /// <summary>
    /// IPollProcessor classes can process (or route) Subsription Requests / Poll data
    /// The class does not need to handle Subscription or poll itself, it may just forward the request on to another class
    /// <typeparam name="TPollType">The type of the report</typeparam>
    /// </summary>
    public interface IPollProcessor<TPollType> : IObservableBase
    {
        IDisposable Subscribe(InputDescriptor subReq, IObserver<InputReport> observer);
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

    public interface IDevice : IDisposable
    {
        /// <summary>
        /// Switch to / from Bind Mode
        /// </summary>
        /// <param name="state">true = Bind Mode, false = Subscription Mode</param>
        void SetBindModeState(bool state);

        /// <summary>
        /// Adds an observer for Subscription Mode
        /// </summary>
        /// <param name="subReq">The descriptor that describes the device and input</param>
        /// <param name="observer">The observer that will handle input reports from the specified input</param>
        /// <returns></returns>
        IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputReport> observer);

        /// <summary>
        /// Adds an observer for Bind Mode
        /// </summary>
        /// <param name="observer">The observer that will handle input reports while in Bind Mode</param>
        /// <returns></returns>
        IDisposable Subscribe(IObserver<InputReport> observer);
    }

    public interface IObservableBase
    {
        int GetObserverCount();
    }
}
