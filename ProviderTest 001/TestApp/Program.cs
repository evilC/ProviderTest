using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using DirectInput;
using XInput;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var di = new DiProvider();
            var xi = new XiProvider();

            var axis1 = new BindingDescriptor(BindingType.Axis, 0);
            var axis2 = new BindingDescriptor(BindingType.Axis, 1);
            var axis6 = new BindingDescriptor(BindingType.Axis, 5);
            var button1 = new BindingDescriptor(BindingType.Button, 0);
            var button2 = new BindingDescriptor(BindingType.Button, 1);
            var button128 = new BindingDescriptor(BindingType.Button, 127);
            var pov1Up = new BindingDescriptor(BindingType.POV, 0, 0);
            var pov1Right = new BindingDescriptor(BindingType.POV, 0, 1);
            var pov1Down = new BindingDescriptor(BindingType.POV, 0, 2);
            var pov1Left = new BindingDescriptor(BindingType.POV, 0, 3);
            var pov2Up = new BindingDescriptor(BindingType.POV, 1, 0);
            var pov2Right = new BindingDescriptor(BindingType.POV, 1, 1);
            var pov2Down = new BindingDescriptor(BindingType.POV, 1, 2);
            var pov2Left = new BindingDescriptor(BindingType.POV, 1, 3);

            var divJoy = new DeviceDescriptor("VID_1234&PID_BEAD");
            var diT16K = new DeviceDescriptor("VID_044F&PID_B10A");
            var xbox1 = new DeviceDescriptor("Xbox");

            //xi.SubscribeBindMode(xbox1, new BindModeObserver("Bind Mode:"));
            //xi.SetBindModeState(xbox1, true);
            //xi.SubscribeInput(new InputDescriptor(xbox1, button1), new TestObserver("XBox Pad 1 Button A"));
            //xi.SubscribeInput(new InputDescriptor(xbox1, pov1Down), new TestObserver("XBox Pad 1 Dpad Down"));

            var xbm = di.SubscribeBindMode(divJoy, new BindModeObserver("Bind Mode: "));

            var b1 = di.SubscribeInput(new InputDescriptor(divJoy, button1), new TestObserver("DI vJoy Button 1"));
            //b1.Dispose();
            //xbm.Dispose();
            di.SubscribeInput(new InputDescriptor(divJoy, button2), new TestObserver("DI vJoy Button 2"));
            di.SubscribeInput(new InputDescriptor(divJoy, button128), new TestObserver("DI vJoy Button 128"));
            di.SubscribeInput(new InputDescriptor(divJoy, pov1Down), new TestObserver("DI vJoy POV 1 Down"));
            di.SubscribeInput(new InputDescriptor(divJoy, pov1Left), new TestObserver("DI vJoy POV 1 Left"));
            di.SubscribeInput(new InputDescriptor(divJoy, pov2Down), new TestObserver("DI vJoy POV 2 Down"));
            di.SubscribeInput(new InputDescriptor(divJoy, pov2Left), new TestObserver("DI vJoy POV 2 Left"));

            //di.SubscribeBindMode(divJoy, new BindModeObserver("Bind Mode:"));
            //di.SetBindModeState(divJoy, true);

            //di.SubscribeInput(new InputDescriptor(diT16K, button1), new TestObserver("DI T16k Button 1"));
            //di.SubscribeInput(new InputDescriptor(diT16K, axis1), new TestObserver("DI T16k Axis X"));

            Console.ReadLine();
        }
    }

    public class TestObserver : IObserver<InputModeReport>
    {
        public string Name { get; }

        public TestObserver(string name)
        {
            Name = name;
        }

        public void OnNext(InputModeReport state)
        {
            Console.WriteLine($"[{Name}] State: {state.InputDescriptor.BindingDescriptor}, Value: {state.Value}");
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }

    public class BindModeObserver : IObserver<InputModeReport>
    {
        public string Name { get; }

        public BindModeObserver(string name)
        {
            Name = name;
        }

        public void OnNext(InputModeReport state)
        {
            Console.WriteLine($"[{Name}] State: {state.InputDescriptor.BindingDescriptor}, Value: {state.Value}");
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }

}
