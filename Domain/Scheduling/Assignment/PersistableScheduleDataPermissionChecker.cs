using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class PersistableScheduleDataPermissionChecker : IPersistableScheduleDataPermissionChecker
	{
		private readonly ICurrentAuthorization _currentAuthorization;

		public PersistableScheduleDataPermissionChecker(ICurrentAuthorization currentAuthorization)
		{
			_currentAuthorization = currentAuthorization;
		}

		public IList<IPersistableScheduleData> GetPermittedData(
			IEnumerable<IPersistableScheduleData> persistableScheduleData)
		{
			var permittedData = persistableScheduleData.Where(d =>
			{
				var forAuthorization =
					new PersistableScheduleDataForAuthorization(d);
				return _currentAuthorization.Current().IsPermitted(
					forAuthorization.FunctionPath,
					forAuthorization.DateOnly,
					forAuthorization.Person);
			}).ToList();
			return permittedData;
		}

		public bool IsModifyPersonAssPermitted(DateOnly dateOnly, IPerson person)
		{
			return _currentAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, dateOnly, person);
		}
	}

	public interface IPersistableScheduleDataPermissionChecker
	{
		IList<IPersistableScheduleData> GetPermittedData(
		IEnumerable<IPersistableScheduleData> persistableScheduleData);

		bool IsModifyPersonAssPermitted(DateOnly dateOnly, IPerson person);
	}
}