using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Factory for StatisticRepository.
	/// Tested from tests for repositories
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-04-29
	/// </remarks>
	public static class StatisticRepositoryFactory
	{
		/// <summary>
		/// Creates a repository instance.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-04-29
		/// </remarks>
		public static IStatisticRepository Create()
		{
			ITeleoptiIdentity identity = null;
			var principal = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal;
			if (principal != null)
			{
				identity = principal.Identity as ITeleoptiIdentity;
			}
			if (identity == null || identity.DataSource.Analytics == null)
				return new StatisticRepositoryEmpty();

			return new StatisticRepository();
		}
	}
}
