using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Options
{
	public class MultiplicatorDefinitionSetController : ApiController
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly INow _now;
		private readonly IMultiplicatorDefinitionSetProvider _multiplicatorDefinitionSetProvider;

		public MultiplicatorDefinitionSetController(ILoggedOnUser loggedOnUser, INow now,
			IMultiplicatorDefinitionSetProvider multiplicatorDefinitionSetProvider)
		{
			_loggedOnUser = loggedOnUser;
			_now = now;
			_multiplicatorDefinitionSetProvider = multiplicatorDefinitionSetProvider;
		}

		[UnitOfWork, HttpGet, Route("api/MultiplicatorDefinitionSet/Overtime")]
		public virtual IList<MultiplicatorDefinitionSetViewModel> FetchOvertimeMultiplicatorDefinitionSets()
		{
			return _multiplicatorDefinitionSetProvider.GetAllOvertimeDefinitionSets();
		}

		[UnitOfWork, HttpGet, Route("api/MultiplicatorDefinitionSet/Mine")]
		public virtual IList<MultiplicatorDefinitionSetViewModel> FetchMyMultiplicatorDefinitionSets()
		{
			var person = _loggedOnUser.CurrentUser();
			if (person == null)
			{
				return new List<MultiplicatorDefinitionSetViewModel>();
			}

			var timezone = person.PermissionInformation.DefaultTimeZone();
			var today = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timezone));
			return _multiplicatorDefinitionSetProvider.GetDefinitionSets(person, today);
		}
	}
}
