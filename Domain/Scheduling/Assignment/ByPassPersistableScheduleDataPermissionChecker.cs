using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ByPassPersistableScheduleDataPermissionChecker : IPersistableScheduleDataPermissionChecker
	{
		public IList<IPersistableScheduleData> GetPermittedData(IEnumerable<IPersistableScheduleData> persistableScheduleData)
		{
			var authorization = PrincipalAuthorization.Current();
			var permittedData = persistableScheduleData.Where(d =>
			{
				var forAuthorization =
					new PersistableScheduleDataForAuthorization(d);
				if (forAuthorization.FunctionPath != DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence &&
					forAuthorization.FunctionPath != DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment)
					return authorization.IsPermitted(
						forAuthorization.FunctionPath,
						forAuthorization.DateOnly,
						forAuthorization.Person);
				return true;
			}).ToList();
			return permittedData;
		}

		public bool IsModifyPersonAssPermitted(DateOnly dateOnly, IPerson person)
		{
			return true;
		}
	}
}