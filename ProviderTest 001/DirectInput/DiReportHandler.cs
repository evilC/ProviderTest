using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using SharpDX.DirectInput;

namespace DirectInput
{
    public class DiReportHandler : ReportHandler
    {
        public override string Title => "DirectInput (Core)";

        public override string Description => "Allows reading of generic joysticks.";

        public override string ProviderName => "SharpDX_DirectInput";

        public override string Api => "DirectInput";

        // Maps SharpDX "Offsets" (Input Identifiers) to both iinput type and input index (eg x axis to axis 1)
        public static Dictionary<BindingType, List<JoystickOffset>> directInputMappings = new Dictionary<BindingType, List<JoystickOffset>>
        {
                {
                    BindingType.Axis, new List<JoystickOffset>
                    {
                        JoystickOffset.X,
                        JoystickOffset.Y,
                        JoystickOffset.Z,
                        JoystickOffset.RotationX,
                        JoystickOffset.RotationY,
                        JoystickOffset.RotationZ,
                        JoystickOffset.Sliders0,
                        JoystickOffset.Sliders1
                    }
                },
                {
                    BindingType.Button, new List<JoystickOffset>
                    {
                        JoystickOffset.Buttons0, JoystickOffset.Buttons1, JoystickOffset.Buttons2, JoystickOffset.Buttons3, JoystickOffset.Buttons4,
                        JoystickOffset.Buttons5, JoystickOffset.Buttons6, JoystickOffset.Buttons7, JoystickOffset.Buttons8, JoystickOffset.Buttons9, JoystickOffset.Buttons10,
                        JoystickOffset.Buttons11, JoystickOffset.Buttons12, JoystickOffset.Buttons13, JoystickOffset.Buttons14, JoystickOffset.Buttons15, JoystickOffset.Buttons16,
                        JoystickOffset.Buttons17, JoystickOffset.Buttons18, JoystickOffset.Buttons19, JoystickOffset.Buttons20, JoystickOffset.Buttons21, JoystickOffset.Buttons22,
                        JoystickOffset.Buttons23, JoystickOffset.Buttons24, JoystickOffset.Buttons25, JoystickOffset.Buttons26, JoystickOffset.Buttons27, JoystickOffset.Buttons28,
                        JoystickOffset.Buttons29, JoystickOffset.Buttons30, JoystickOffset.Buttons31, JoystickOffset.Buttons32, JoystickOffset.Buttons33, JoystickOffset.Buttons34,
                        JoystickOffset.Buttons35, JoystickOffset.Buttons36, JoystickOffset.Buttons37, JoystickOffset.Buttons38, JoystickOffset.Buttons39, JoystickOffset.Buttons40,
                        JoystickOffset.Buttons41, JoystickOffset.Buttons42, JoystickOffset.Buttons43, JoystickOffset.Buttons44, JoystickOffset.Buttons45, JoystickOffset.Buttons46,
                        JoystickOffset.Buttons47, JoystickOffset.Buttons48, JoystickOffset.Buttons49, JoystickOffset.Buttons50, JoystickOffset.Buttons51, JoystickOffset.Buttons52,
                        JoystickOffset.Buttons53, JoystickOffset.Buttons54, JoystickOffset.Buttons55, JoystickOffset.Buttons56, JoystickOffset.Buttons57, JoystickOffset.Buttons58,
                        JoystickOffset.Buttons59, JoystickOffset.Buttons60, JoystickOffset.Buttons61, JoystickOffset.Buttons62, JoystickOffset.Buttons63, JoystickOffset.Buttons64,
                        JoystickOffset.Buttons65, JoystickOffset.Buttons66, JoystickOffset.Buttons67, JoystickOffset.Buttons68, JoystickOffset.Buttons69, JoystickOffset.Buttons70,
                        JoystickOffset.Buttons71, JoystickOffset.Buttons72, JoystickOffset.Buttons73, JoystickOffset.Buttons74, JoystickOffset.Buttons75, JoystickOffset.Buttons76,
                        JoystickOffset.Buttons77, JoystickOffset.Buttons78, JoystickOffset.Buttons79, JoystickOffset.Buttons80, JoystickOffset.Buttons81, JoystickOffset.Buttons82,
                        JoystickOffset.Buttons83, JoystickOffset.Buttons84, JoystickOffset.Buttons85, JoystickOffset.Buttons86, JoystickOffset.Buttons87, JoystickOffset.Buttons88,
                        JoystickOffset.Buttons89, JoystickOffset.Buttons90, JoystickOffset.Buttons91, JoystickOffset.Buttons92, JoystickOffset.Buttons93, JoystickOffset.Buttons94,
                        JoystickOffset.Buttons95, JoystickOffset.Buttons96, JoystickOffset.Buttons97, JoystickOffset.Buttons98, JoystickOffset.Buttons99, JoystickOffset.Buttons100,
                        JoystickOffset.Buttons101, JoystickOffset.Buttons102, JoystickOffset.Buttons103, JoystickOffset.Buttons104, JoystickOffset.Buttons105, JoystickOffset.Buttons106,
                        JoystickOffset.Buttons107, JoystickOffset.Buttons108, JoystickOffset.Buttons109, JoystickOffset.Buttons110, JoystickOffset.Buttons111, JoystickOffset.Buttons112,
                        JoystickOffset.Buttons113, JoystickOffset.Buttons114, JoystickOffset.Buttons115, JoystickOffset.Buttons116, JoystickOffset.Buttons117, JoystickOffset.Buttons118,
                        JoystickOffset.Buttons119, JoystickOffset.Buttons120, JoystickOffset.Buttons121, JoystickOffset.Buttons122, JoystickOffset.Buttons123, JoystickOffset.Buttons124,
                        JoystickOffset.Buttons125, JoystickOffset.Buttons126, JoystickOffset.Buttons127
                    }
                },
                {
                    BindingType.POV, new List<JoystickOffset>
                    {
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers3
                    }
                }
        };

        public static List<string> povDirections = new List<string> { "Up", "Right", "Down", "Left" };

        public DiReportHandler()
        {
            for (var povNum = 0; povNum < 4; povNum++)
            {
                PovBindingInfos[povNum] = new List<BindingReport>();
                for (var povDir = 0; povDir < 4; povDir++)
                {
                    PovBindingInfos[povNum].Add(new BindingReport
                    {
                        Title = povDirections[povDir],
                        Category = BindingCategory.Momentary,
                        BindingDescriptor = new BindingDescriptor(BindingType.POV, povNum, povDir)
                    });
                }
            }


        }

        public override List<DeviceReport> GetInputDeviceReports()
        {
            QueryDevices();
            return _deviceReports;
        }

        public override List<DeviceReport> GetOutputDeviceReports()
        {
            return null;
        }

        /*
        public override DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            foreach (var deviceReport in _deviceReports)
            {
                if (deviceReport.DeviceDescriptor.DeviceHandle == subReq.DeviceDescriptor.DeviceHandle && deviceReport.DeviceDescriptor.DeviceInstance == subReq.DeviceDescriptor.DeviceInstance)
                {
                    return deviceReport;
                }
            }
            return null;
        }

        public override DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return null;
        }
        */

        public override void RefreshDevices()
        {
            QueryDevices();
        }

        #region Device Querying
        private List<DeviceReport> _deviceReports;
        private static Dictionary<string, List<DeviceInstance>> _devicesList;
        private static readonly List<BindingReport>[] PovBindingInfos = new List<BindingReport>[4];
        private static readonly SharpDX.DirectInput.DirectInput DirectInput = new SharpDX.DirectInput.DirectInput();

        private void QueryDevices()
        {
            _devicesList = new Dictionary<string, List<DeviceInstance>>();

            _deviceReports = new List<DeviceReport>();

            //// ToDo: device list should be returned in handle order for duplicate devices
            //var diDeviceInstances = DirectInput.GetDevices();

            //var unsortedInstances = new Dictionary<string, List<DeviceInstance>>();
            //foreach (var device in diDeviceInstances)
            //{
            //    if (!Lookups.IsStickType(device))
            //        continue;
            //    var joystick = new Joystick(DirectInput, device.InstanceGuid);
            //    joystick.Acquire();

            //    var handle = Lookups.JoystickToHandle(joystick);

            //    if (!unsortedInstances.ContainsKey(handle))
            //    {
            //        unsortedInstances[handle] = new List<DeviceInstance>();
            //    }
            //    unsortedInstances[handle].Add(device);
            //    joystick.Unacquire();
            //}

            //foreach (var diDeviceInstance in unsortedInstances)
            //{
            //    _devicesList.Add(diDeviceInstance.Key, Lookups.OrderDevices(diDeviceInstance.Key, diDeviceInstance.Value));
            //}

            foreach (var deviceList in DiWrapper.Instance.ConnectedDevices)
            {
                var deviceHandle = deviceList.Key;
                for (var index = 0; index < deviceList.Value.Count; index++)
                {
                    var deviceGuid = deviceList.Value[index];

                    var joystick = new Joystick(DirectInput, deviceGuid);
                    joystick.Acquire();

                    var device = new DeviceReport
                    {
                        DeviceName = joystick.Information.ProductName,
                        DeviceDescriptor = new DeviceDescriptor(deviceHandle,index)
                    };

                    // ----- Axes -----
                    if (joystick.Capabilities.AxeCount > 0)
                    {
                        var axisInfo = new DeviceReportNode
                        {
                            Title = "Axes"
                        };

                        // SharpDX tells us how many axes there are, but not *which* axes.
                        // Enumerate all possible DI axes and check to see if this stick has each axis
                        for (var i = 0; i < directInputMappings[BindingType.Axis].Count; i++)
                        {
                            try
                            {
                                var deviceInfo =
                                    joystick.GetObjectInfoByName(directInputMappings[BindingType.Axis][i]   // this bit will go boom if the axis does not exist
                                        .ToString());
                                axisInfo.Bindings.Add(new BindingReport
                                {
                                    Title = deviceInfo.Name,
                                    Category = BindingCategory.Signed,
                                    BindingDescriptor = new BindingDescriptor(BindingType.Axis, i, 0)
                                });
                            }
                            catch
                            {
                                // axis does not exist
                            }
                        }

                        device.Nodes.Add(axisInfo);
                    }

                    // ----- Buttons -----
                    var length = joystick.Capabilities.ButtonCount;
                    if (length > 0)
                    {
                        var buttonInfo = new DeviceReportNode
                        {
                            Title = "Buttons"
                        };
                        for (var btn = 0; btn < length; btn++)
                        {
                            buttonInfo.Bindings.Add(new BindingReport
                            {
                                Title = (btn + 1).ToString(),
                                Category = BindingCategory.Momentary,
                                BindingDescriptor = new BindingDescriptor(BindingType.Button, btn, 0)
                            });
                        }

                        device.Nodes.Add(buttonInfo);
                    }

                    // ----- POVs -----
                    var povCount = joystick.Capabilities.PovCount;
                    if (povCount > 0)
                    {
                        var povsInfo = new DeviceReportNode
                        {
                            Title = "POVs"
                        };
                        for (var p = 0; p < povCount; p++)
                        {
                            var povInfo = new DeviceReportNode
                            {
                                Title = "POV #" + (p + 1),
                                Bindings = PovBindingInfos[p]
                            };
                            povsInfo.Nodes.Add(povInfo);
                        }
                        device.Nodes.Add(povsInfo);
                    }

                    _deviceReports.Add(device);


                    joystick.Unacquire();
                }

            }
        }
        #endregion

    }
}
