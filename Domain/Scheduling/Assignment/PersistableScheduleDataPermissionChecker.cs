using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class PersistableScheduleDataPermissionChecker : IPersistableScheduleDataPermissionChecker
	{
		private readonly IPermissionProvider _permissionProvider;

		public PersistableScheduleDataPermissionChecker(IPermissionProvider permissionProvider)
		{
			_permissionProvider = permissionProvider;
		}

		public IList<IPersistableScheduleData> GetPermittedData(
			IEnumerable<IPersistableScheduleData> persistableScheduleData)
		{
			var permittedData = persistableScheduleData.Where(d =>
			{
				var forAuthorization =
					new PersistableScheduleDataForAuthorization(d);
				return _permissionProvider.HasPersonPermission(
					forAuthorization.FunctionPath,
					forAuthorization.DateOnly,
					forAuthorization.Person);
			}).ToList();
			return permittedData;
		}

		public bool IsModifyPersonAssPermitted(DateOnly dateOnly, IPerson person)
		{
			return _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, dateOnly, person);
		}
	}

	public interface IPersistableScheduleDataPermissionChecker
	{
		IList<IPersistableScheduleData> GetPermittedData(
		IEnumerable<IPersistableScheduleData> persistableScheduleData);

		bool IsModifyPersonAssPermitted(DateOnly dateOnly, IPerson person);
	}
}