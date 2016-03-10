namespace Teleopti.Interfaces.Infrastructure
{
	public interface IHttpServer
	{
		void Post(string uri, object thing);
		void PostOrThrow(string uri, object thing);
		string GetOrThrow(string uri);
	}
}