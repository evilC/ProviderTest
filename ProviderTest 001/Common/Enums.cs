﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public enum BindingType
    {
        Axis,
        Button,
        POV
    }

    /// <summary>
    /// Describes the reporting style of a Binding
    /// Only used for the back-end to report to the front-end how to work with the binding
    /// </summary>
    public enum BindingCategory { Momentary, Event, Signed, Unsigned, Delta }
    //public enum AxisCategory { Signed, Unsigned, Delta }
    //public enum ButtonCategory { Momentary, Event }
    //public enum POVCategory { POV1, POV2, POV3, POV4 }

    public enum PollMode
    {
        Subscription,
        Bind
    }

}
