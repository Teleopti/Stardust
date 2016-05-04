using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class PersistableScheduleDataPermissionChecker : IPersistableScheduleDataPermissionChecker
	{
		private readonly IPrincipalAuthorization _authorization;

		public PersistableScheduleDataPermissionChecker(IPrincipalAuthorization authorization)
		{
			_authorization = authorization;
		}

		public IList<IPersistableScheduleData> GetPermittedData(
		IEnumerable<IPersistableScheduleData> persistableScheduleData)
		{
			var permittedData = persistableScheduleData.Where(d =>
			{
				var forAuthorization =
	new PersistableScheduleDataForAuthorization(d);
				return _authorization.IsPermitted(
	forAuthorization.FunctionPath,
	forAuthorization.DateOnly,
	forAuthorization.Person);
			}).ToList();
			return permittedData;
		}

	}


	public interface IPersistableScheduleDataPermissionChecker
	{
		IList<IPersistableScheduleData> GetPermittedData(
		IEnumerable<IPersistableScheduleData> persistableScheduleData);
	}
}