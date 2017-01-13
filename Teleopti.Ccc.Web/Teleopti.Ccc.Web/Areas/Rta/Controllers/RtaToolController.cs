using System.Linq;
using System.Web.Http;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.All)]
	public class RtaToolController : ApiController
	{
		private readonly IDataSourceReader _dataSources;
		private readonly IPersonRepository _persons;
		private readonly IRtaStateGroupRepository _stateGroups;
		private readonly INow _now;

		public RtaToolController(
			IDataSourceReader dataSources,
			IPersonRepository persons,
			IRtaStateGroupRepository stateGroups,
			INow now)
		{
			_dataSources = dataSources;
			_persons = persons;
			_stateGroups = stateGroups;
			_now = now;
		}

		[AnalyticsUnitOfWork, UnitOfWork, HttpGet, Route("RtaTool/Agents/For")]
		public virtual IHttpActionResult GetAgents()
		{
			var today = new DateOnly(_now.UtcDateTime());
			var persons = _persons.FindPeopleInOrganization(new DateOnlyPeriod(today, today), false);
			var dataSources = _dataSources.Datasources();
			var result = (
				from p in persons
				let period = p.Period(today)
				let logon = period.ExternalLogOnCollection.OrderBy(x => x.DataSourceId).FirstOrDefault()
				let external = logon?.AcdLogOnOriginalId
				let d = logon?.DataSourceId
				let datasource = dataSources.FirstOrDefault(x => x.Value == d).Key
				where logon != null
				select new
				{
					Name = p.Name.FirstName + " " + p.Name.LastName,
					UserCode = external,
					DataSource = datasource
				})
				.ToArray();
			return Ok(result);
		}

		[UnitOfWork, HttpGet, Route("RtaTool/PhoneStates/For")]
		public virtual IHttpActionResult GetPhoneStates()
		{
			return Ok(_stateGroups.LoadAllCompleteGraph()
				.Where(x => !x.StateCollection.IsNullOrEmpty())
				.OrderByDescending(x => x.Available)
				.ThenBy(x => x.DefaultStateGroup)
				.ThenBy(x => x.IsLogOutState)
				.Select(x => new
				{
					Name = x.Name,
					Code = x.StateCollection.First().StateCode
				})
				.ToArray());
		}
	}
}