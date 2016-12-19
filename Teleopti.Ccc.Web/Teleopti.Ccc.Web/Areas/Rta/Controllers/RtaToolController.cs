using System.Linq;
using System.Web.Http;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
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

		[LocalHostAccess, AnalyticsUnitOfWork, UnitOfWork, HttpGet, Route("RtaTool/Agents/For")]
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
				let dataSource = logon?.DataSourceId
				let dataScoure2 = dataSources.FirstOrDefault(x => x.Value == dataSource).Key
				where logon != null
				select new
				{
					Name = p.Name.FirstName + " " + p.Name.LastName,
					UserCode = external,
					DataSource = dataScoure2
				})
				.ToArray();
			return Ok(result);
		}

		[LocalHostAccess, UnitOfWork, HttpGet, Route("RtaTool/PhoneStates/For")]
		public virtual IHttpActionResult GetPhoneStates()
		{
			return Ok(_stateGroups.LoadAllCompleteGraph()
				.Where(x => !x.StateCollection.IsNullOrEmpty())
				.Select(x => new
				{
					Name = x.Name,
					State = x.StateCollection.First().StateCode
				})
				.OrderBy(x => x.Name)
				.ToArray());
		}
	}
}