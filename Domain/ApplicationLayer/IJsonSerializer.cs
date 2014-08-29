namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IJsonSerializer
	{
		string SerializeObject(object value);
	}
}