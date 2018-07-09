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

        public DiPovProcessor(BindingDescriptor bindingDescriptor)
        {
            for (var i = 0; i < 4; i++)
            {
                var descriptor = new BindingDescriptor(BindingType.POV, bindingDescriptor.Index, i);
                _directionProcessors.Add(new DiPovDirectionProcessor(descriptor));
            }
        }

        public IDisposable Subscribe(BindingDescriptor bindingDescriptor, IObserver<InputModeReport> observer)
        {
            return _directionProcessors[bindingDescriptor.SubIndex].Subscribe(bindingDescriptor, observer);
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
    }
}
