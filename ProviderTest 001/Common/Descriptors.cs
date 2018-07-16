﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public struct InputDescriptor
    {
        public DeviceDescriptor DeviceDescriptor { get; }
        public BindingDescriptor BindingDescriptor { get; }

        public InputDescriptor(DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor)
        {
            DeviceDescriptor = deviceDescriptor;
            BindingDescriptor = bindingDescriptor;
        }
    }

    public struct BindingDescriptor
    {
        public BindingDescriptor(BindingType type, int index, int subIndex = 0)
        {
            Type = type;
            Index = index;
            SubIndex = subIndex;
        }

        /// <summary>
        /// The Type of the Binding - ie Button / Axis / POV
        /// </summary>
        public BindingType Type { get; }

        /// <summary>
        /// The Type-specific Index of the Binding
        /// This is often a Sparse Index (it may often be a BitMask value) ...
        /// ... as it is often refers to an enum value in a Device Report
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The Type-specific SubIndex of the Binding
        /// This is typically unused, but if used generally represents a derived or optional value
        /// For example:
        ///     With each POV reporting natively as an Angle (Like an Axis)
        ///     But in IOWrapper, bindings are to a *Direction* of a POV (As if it were a button)
        ///     So we need to specify the angle of that direction in SubIndex...
        ///     ... as well as the POV# in Index. Directinput supports 4 POVs
        /// </summary>
        public int SubIndex { get; }

        public override string ToString()
        {
            return $"Type: {Type}, Index: {Index}, SubIndex: {SubIndex}";
        }
    }

    /// <summary>
    /// Identifies the Provider responsible for handling the Binding
    /// </summary>
    public class ProviderDescriptor
    {
        /// <summary>
        /// The API implementation that handles this input
        /// This should be unique
        /// </summary>
        public string ProviderName { get; set; }
    }

    /// <summary>
    /// Identifies a device within a Provider
    /// </summary>
    public struct DeviceDescriptor
    {
        public DeviceDescriptor(string deviceHandle, int deviceInstance = 0)
        {
            DeviceHandle = deviceHandle;
            DeviceInstance = deviceInstance;
        }
        /// <summary>
        /// A way to uniquely identify a device instance via it's API
        /// Note that ideally all providers implementing the same API should ideally generate the same device handles
        /// For something like RawInput or DirectInput, this would likely be based on VID/PID
        /// For an ordered API like XInput, this would just be controller number
        /// </summary>
        public string DeviceHandle { get; }

        public int DeviceInstance { get; }
    }
}
