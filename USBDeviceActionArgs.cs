namespace USBNotification
{
    public class USBDeviceActionArgs(string port_name, USBDeviceActions action)
    {
        public string PortName { get; set; } = port_name;
        public USBDeviceActions Action { get; set; } = action;
    }

    public delegate void USBDeviceConnectionChangedHandler(object sender, USBDeviceActionArgs e);
}
