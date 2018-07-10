using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using SharpDX.XInput;

namespace XInput
{
    class XiButtonProcessor : IPollProcessor<State>, IObservableInput<InputModeReport>
    {
        public EventHandler<InputReportEventArgs> OnBindMode;
        private readonly InputDescriptor _inputDescriptor;
        private readonly List<IObserver<InputModeReport>> _observers = new List<IObserver<InputModeReport>>();
        private readonly GamepadButtonFlags _gamepadButtonFlag;
        private int _lastValue;

        private static readonly List<GamepadButtonFlags> _buttonFlags = new List<GamepadButtonFlags>
        {
            GamepadButtonFlags.A, GamepadButtonFlags.B, GamepadButtonFlags.X, GamepadButtonFlags.Y,
            GamepadButtonFlags.LeftShoulder, GamepadButtonFlags.RightShoulder, GamepadButtonFlags.LeftThumb, GamepadButtonFlags.RightThumb,
            GamepadButtonFlags.Back, GamepadButtonFlags.Start,
            GamepadButtonFlags.DPadUp, GamepadButtonFlags.DPadRight, GamepadButtonFlags.DPadDown, GamepadButtonFlags.DPadLeft
        };

        public XiButtonProcessor(InputDescriptor inputDescriptor, EventHandler<InputReportEventArgs> bindModeHandler)
        {
            OnBindMode += bindModeHandler;
            _inputDescriptor = inputDescriptor;
            var index = inputDescriptor.BindingDescriptor.Type == BindingType.Button
                ? inputDescriptor.BindingDescriptor.Index
                : inputDescriptor.BindingDescriptor.SubIndex + 10;
            _gamepadButtonFlag = _buttonFlags[index];
        }

        #region Interfaces
        public int GetObserverCount()
        {
            return _observers.Count;
        }

        public IDisposable Subscribe(InputDescriptor subReq, IObserver<InputModeReport> observer)
        {
            return Subscribe(observer);
        }

        public void ProcessSubscriptionMode(State thisState)
        {
            ProcessPoll(PollMode.Subscription, thisState);
        }

        public void ProcessBindMode(State thisState)
        {
            ProcessPoll(PollMode.Bind, thisState);
        }

        public void ProcessPoll(PollMode mode, State thisState)
        {
            var thisValue = GetValue(thisState);
            if (thisValue == _lastValue) return;
            var report = BuildReport(thisState);

            switch (mode)
            {
                case PollMode.Bind:
                    OnBindMode(this, new InputReportEventArgs(report));
                    break;
                case PollMode.Subscription:
                    foreach (var observer in _observers)
                    {
                        observer.OnNext(report);
                    }
                    break;
                default:
                    throw new Exception($"Unknown Poll Mode: {mode}");
            }

            _lastValue = thisValue;

        }

        private int GetValue(State thisState)
        {
            return (_gamepadButtonFlag & thisState.Gamepad.Buttons) == _gamepadButtonFlag ? 1 : 0;
        }

        private InputModeReport BuildReport(State state)
        {
            return new InputModeReport(_inputDescriptor, GetValue(state));
        }

        public IDisposable Subscribe(IObserver<InputModeReport> observer)
        {
            _observers.Add(observer);
            return new ObservableUnsubscriber<InputModeReport>(_observers, observer);
        }
        #endregion
    }
}
