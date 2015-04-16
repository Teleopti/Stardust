namespace Teleopti.Interfaces
{
	public class ToStringSerializer : IJsonSerializer
	{
		public string SerializeObject(object value)
		{
			return value.ToString();
		}
	}
}