using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
	public class PerformanceToolApiController : ApiController
	{
		private readonly IPerformanceCounter _performanceCounter;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IPersonGenerator _personGenerator;
		private readonly IStateGenerator _stateGenerator;

		public PerformanceToolApiController(IPerformanceCounter performanceCounter,
			ICurrentBusinessUnit businessUnit,
			ICurrentDataSource currentDataSource,
			IPersonGenerator personGenerator,
			IStateGenerator stateGenerator)
		{
			_performanceCounter = performanceCounter;
			_businessUnit = businessUnit;
			_currentDataSource = currentDataSource;
			_personGenerator = personGenerator;
			_stateGenerator = stateGenerator;
		}

		[UnitOfWork, HttpGet, Route("api/performancetool/ResetPerformanceCounter")]
		public virtual IHttpActionResult ResetPerformanceCounter(int iterationCount)
		{
			_performanceCounter.Limit = iterationCount;
			_performanceCounter.BusinessUnitId = _businessUnit.Current().Id.GetValueOrDefault();
			_performanceCounter.DataSource = _currentDataSource.CurrentName();
			_performanceCounter.ResetCount();
			return Ok("ok");
		}

		[UnitOfWork, HttpGet, Route("api/performancetool/ManageAdherenceLoadTest")]
		public virtual IHttpActionResult ManageAdherenceLoadTest(int iterationCount)
		{
			var personData = _personGenerator.Generate(iterationCount);
			var stateCodes = _stateGenerator.Generate(iterationCount);
			return Ok(new AdherenceLoadTestParameters(personData.Persons, stateCodes, personData.TeamId));
		}

		[UnitOfWork, HttpGet, Route("api/performancetool/ClearManageAdherenceLoadTest")]
		public virtual IHttpActionResult ClearManageAdherenceLoadTest(int iterationCount)
		{
			_personGenerator.Clear(iterationCount);
			return Ok("ok");
		}
	}

	public struct AdherenceLoadTestParameters
	{
		public AdherenceLoadTestParameters(IEnumerable<PersonWithExternalLogon> persons, IEnumerable<string> states,Guid teamId) : this()
		{
			Persons = persons;
			States = states;
			TeamId = teamId;
		}

		public IEnumerable<PersonWithExternalLogon> Persons { get; private set; }
		public IEnumerable<string> States { get; private set; }
		public Guid TeamId { get; private set; }
	}
}