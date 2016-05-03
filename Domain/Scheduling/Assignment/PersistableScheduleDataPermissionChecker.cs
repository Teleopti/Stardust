using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class PersistableScheduleDataPermissionChecker : IPersistableScheduleDataPermissionChecker
	{
		public IList<IPersistableScheduleData> GetPermittedData(
		IEnumerable<IPersistableScheduleData> persistableScheduleData)
		{
			var authorization = PrincipalAuthorization.Instance();
			var permittedData = persistableScheduleData.Where(d =>
			{
				var forAuthorization =
	new PersistableScheduleDataForAuthorization(d);
				return authorization.IsPermitted(
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