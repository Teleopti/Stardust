namespace Teleopti.Ccc.Domain.InterfaceLegacy
{
	public class ToStringSerializer : IJsonSerializer
	{
		public string SerializeObject(object value)
		{
			return value.ToString();
		}
	}
}