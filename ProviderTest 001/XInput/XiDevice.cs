using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.XInput;

namespace XInput
{
    class XiDevice
    {
        private readonly Dictionary<(BindingType, int, int), IPollProcessor<State>> _pollProcessors = new Dictionary<(BindingType, int, int), IPollProcessor<State>>();
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly List<IObserver<InputModeReport>> _bindModeObservers = new List<IObserver<InputModeReport>>();

        private readonly Controller _device;

        public XiDevice(DeviceDescriptor deviceDescriptor)
        {
            _device = new Controller((UserIndex)deviceDescriptor.DeviceInstance);

            BuildPollProcessors();
            _deviceDescriptor = deviceDescriptor;

            var pollThread = new Thread(PollThread);
            pollThread.Start();
        }

        private void PollThread()
        {
            while (true)
            {
                var thisState = _device.GetState();

                // Buttons
                for (var i = 0; i < 10; i++)
                {
                    var buttonTuple = BuildTuple(BindingType.Button, i);
                    if (_pollProcessors[buttonTuple].GetObserverCount() == 0) continue;

                    _pollProcessors[buttonTuple].ProcessSubscriptionMode(thisState);
                }

                Thread.Sleep(10);
            }
        }

        public (BindingType, int, int) BuildTuple(BindingType bindingType, int index, int subIndex = 0)
        {
            return (bindingType, index, subIndex);
        }


        private void BuildPollProcessors()
        {
            for (var i = 0; i < 14; i++)
            {
                var descriptor = new InputDescriptor(_deviceDescriptor, new BindingDescriptor(BindingType.Button, i));
                _pollProcessors[descriptor.BindingDescriptor.ToTuple()] = new XiButtonProcessor(descriptor, OnBindMode);
            }
        }

        public IDisposable SubscribeInput(InputDescriptor subReq, IObserver<InputModeReport> observer)
        {
            return _pollProcessors[subReq.BindingDescriptor.ToTuple()].Subscribe(subReq, observer);
        }

        private void OnBindMode(object sender, InputReportEventArgs inputReportEventArgs)
        {
            foreach (var bindModeObserver in _bindModeObservers)
            {
                bindModeObserver.OnNext(inputReportEventArgs.InputModeReport);
            }
        }
    }
}
