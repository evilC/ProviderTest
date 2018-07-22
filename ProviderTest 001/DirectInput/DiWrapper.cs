using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using SharpDX.DirectInput;

namespace DirectInput
{
    public class DiWrapper
    {
        public ConcurrentDictionary<string, List<Guid>> ConnectedDevices { get; } = new ConcurrentDictionary<string, List<Guid>>();

        #region Singleton setup
        private static DiWrapper _instance;
        public static DiWrapper Instance => _instance ?? (_instance = new DiWrapper());
        public static SharpDX.DirectInput.DirectInput DiInstance { get; } = new SharpDX.DirectInput.DirectInput();
        #endregion

        public DiWrapper()
        {
            RefreshConnectedDevices();
        }

        public void RefreshConnectedDevices()
        {
            //ConnectedDevices = new ConcurrentDictionary<string, List<Guid>>();
            var diDeviceInstances = DiInstance.GetDevices();
            var connectedHandles = ConnectedDevices.Keys.ToList();
            foreach (var device in diDeviceInstances)
            {
                if (!IsStickType(device))
                    continue;
                var joystick = new Joystick(DiInstance, device.InstanceGuid);
                var handle = JoystickToHandle(joystick);
                if (connectedHandles.Contains(handle)) connectedHandles.Remove(handle);
                if (ConnectedDevices.ContainsKey(handle))
                {
                    if (ConnectedDevices[handle].Contains(device.InstanceGuid))
                    {
                        continue;
                    }
                }
                else
                {
                    ConnectedDevices[handle] = new List<Guid>();
                }
                ConnectedDevices[handle].Add(device.InstanceGuid);
            }

            foreach (var connectedHandle in connectedHandles)
            {
                ConnectedDevices.TryRemove(connectedHandle, out _);
            }
        }

        public Guid DeviceDescriptorToInstanceGuid(DeviceDescriptor deviceDescriptor)
        {
            if (ConnectedDevices.ContainsKey(deviceDescriptor.DeviceHandle)
                && ConnectedDevices[deviceDescriptor.DeviceHandle].Count >= deviceDescriptor.DeviceInstance)
            {
                return ConnectedDevices[deviceDescriptor.DeviceHandle][deviceDescriptor.DeviceInstance];
            }
            throw new Exception($"Could not find device Handle {deviceDescriptor.DeviceHandle}, Instance {deviceDescriptor.DeviceInstance}");
            //return Guid.Empty;
        }

        public static bool IsStickType(DeviceInstance deviceInstance)
        {
            return deviceInstance.Type == DeviceType.Joystick
                    || deviceInstance.Type == DeviceType.Gamepad
                    || deviceInstance.Type == DeviceType.FirstPerson
                    || deviceInstance.Type == DeviceType.Flight
                    || deviceInstance.Type == DeviceType.Driving
                    || deviceInstance.Type == DeviceType.Supplemental;
        }

        public static string JoystickToHandle(Joystick joystick)
        {
            return $"VID_{joystick.Properties.VendorId:X4}&PID_{joystick.Properties.ProductId:X4}";
        }

        //public DeviceDescriptor DeviceGuidToDeviceDescriptor(Guid deviceGuid)
        //{
        //    foreach (var connectedDevice in ConnectedDevices)
        //    {
        //        if (connectedDevice.Value.Contains(deviceGuid))
        //        {
        //            return new DeviceDescriptor {DeviceHandle = connectedDevice.Key, DeviceInstance = connectedDevice.Value.IndexOf(deviceGuid)};
        //        }
        //    }
        //    throw new Exception($"Could not find device GUID {deviceGuid} in Connected Devices list");
        //}
    }
}
