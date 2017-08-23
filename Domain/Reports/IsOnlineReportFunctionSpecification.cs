using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Reports
{
	public class IsOnlineReportFunctionSpecification : Specification<IApplicationFunction>
	{
		private readonly string[] _onlineReportForeignIdList;

		public IsOnlineReportFunctionSpecification()
		{
			_onlineReportForeignIdList = new[] { "0055", "0059", "0064" };
		}

		public override bool IsSatisfiedBy(IApplicationFunction obj)
		{
			return _onlineReportForeignIdList.Contains(obj.ForeignId);
		}
	}
}
