namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IKeyValueStorePersister
	{
		void Update(string key, string value);
		string Get(string key);
		void Delete(string key);
	}
}