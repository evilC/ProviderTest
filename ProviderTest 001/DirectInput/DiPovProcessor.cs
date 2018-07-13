using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using SharpDX.DirectInput;

namespace DirectInput
{
    class DiPovProcessor : IPollProcessor<JoystickUpdate>
    {
        private readonly List<DiPovDirectionProcessor> _directionProcessors = new List<DiPovDirectionProcessor>();
        private readonly EventHandler _observerListEmptyEventHandler;

        public DiPovProcessor(InputDescriptor inputDescriptor, EventHandler observerListEmptyEventHandler, EventHandler<InputReportEventArgs> bindModeHandler)
        {
            _observerListEmptyEventHandler = observerListEmptyEventHandler;
            for (var i = 0; i < 4; i++)
            {
                var descriptor = new InputDescriptor(inputDescriptor.DeviceDescriptor, new BindingDescriptor(BindingType.POV, inputDescriptor.BindingDescriptor.Index, i));
                _directionProcessors.Add(new DiPovDirectionProcessor(descriptor, observerListEmptyEventHandler, bindModeHandler));
            }
        }

        public IDisposable Subscribe(InputDescriptor subReq, IObserver<InputReport> observer)
        {
            return _directionProcessors[subReq.BindingDescriptor.SubIndex].Subscribe(subReq, observer);
        }

        public void ProcessSubscriptionMode(JoystickUpdate state)
        {
            for (var i = 0; i < 4; i++)
            {
                if (_directionProcessors[i].GetObserverCount() > 0)
                {
                    _directionProcessors[i].ProcessSubscriptionMode(state);
                }
            }
        }

        public void ProcessBindMode(JoystickUpdate state)
        {
            for (var i = 0; i < 4; i++)
            {
                _directionProcessors[i].ProcessBindMode(state);
            }
        }

        public int GetObserverCount()
        {
            var count = 0;
            for (var i = 0; i < 4; i++)
            {
                count += _directionProcessors[i].GetObserverCount();
            }
            return count;
        }
    }
}
