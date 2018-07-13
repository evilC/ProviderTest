using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class InputReportEventArgs : EventArgs
    {
        public InputReport InputReport { get; }

        public InputReportEventArgs(InputReport inputReport)
        {
            InputReport = inputReport;
        }
    }

    public class DeviceEmptyEventArgs : EventArgs
    {
        public DeviceDescriptor DeviceDescriptor { get; }

        public DeviceEmptyEventArgs(DeviceDescriptor deviceDescriptor)
        {
            DeviceDescriptor = deviceDescriptor;
        }
    }
}
