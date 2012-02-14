using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IInitializeApplication
	{
		/// <summary>
		/// Setup the application. Should be run once per application.
		/// Is _not_ thread safe. It's the client's responsibility.
		/// </summary>
		/// <param name="clientCache">The client cache.</param>
		/// <param name="xmlDirectory">The directory to nhibernate's conf file(s)</param>
		/// <param name="loadPasswordPolicyService">The password policy loading service</param>
		void Start(IState clientCache, string xmlDirectory, ILoadPasswordPolicyService loadPasswordPolicyService);
	}
}