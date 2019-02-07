namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IJsonEventSerializer
	{
		string SerializeEvent(object value);
	}
}