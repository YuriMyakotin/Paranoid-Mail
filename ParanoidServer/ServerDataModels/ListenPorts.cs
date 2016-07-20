namespace Paranoid
{
	public enum AutoRegistrationModes:int
	{
		Disabled=0,
		EnabledWithTimeout=1,
		EnabledAlways=2
	}
	public class ListenPorts
	{
		public string Ip { get; set; }
		public int Port { get; set; }
		public int AutoRegistration { get; set; }
		public string PrivatePortPassword { get; set; }

	}
}
