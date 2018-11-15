using System;
using System.Globalization;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class PermissionsController : ApiController
	{
		private readonly PermissionsViewModelBuilder _builder;

		public PermissionsController(PermissionsViewModelBuilder builder)
		{
			_builder = builder;
		}

		[UnitOfWork, HttpGet, Route("api/Adherence/Permissions")]
		public virtual IHttpActionResult Load(Guid? personId, string date)
		{
			var dateOnly = new DateOnly(DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None));
			return Ok(_builder.Build(personId, dateOnly));
		}
	}

	public class PermissionsViewModelBuilder
	{
		private readonly ICurrentAuthorization _authorization;
		private readonly IPersonRepository _persons;

		public PermissionsViewModelBuilder(ICurrentAuthorization authorization, IPersonRepository persons)
		{
			_authorization = authorization;
			_persons = persons;
		}

		public PermissionsViewModel Build(Guid? personId, DateOnly? date)
		{
			IPerson person = null;
			if (personId != null)
				person = _persons.Load(personId.Value);
			return new PermissionsViewModel
			{
				HasHistoricalOverviewPermission = isPermitted(DefinedRaptorApplicationFunctionPaths.HistoricalOverview, date, person),
				HasModifyAdherencePermission = isPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAdherence, date, person)
			};
		}

		private bool isPermitted(string permission, DateOnly? date, IPerson person)
		{
			if (date == null)
				return _authorization.Current().IsPermitted(permission);
			return _authorization.Current().IsPermitted(permission, date.Value, person);
		}
	}

	public class PermissionsViewModel
	{
		public bool HasHistoricalOverviewPermission;
		public bool HasModifyAdherencePermission;
	}
}