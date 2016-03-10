using System;
using System.Web.Http;
using Mindscape.Raygun4Net.WebApi;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
	[LocalHostAccess]
	public class DangerousController : ApiController
	{
		private readonly INow _now;
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentUnitOfWork _unitOfWork;

		public DangerousController(INow now,
			ISiteRepository siteRepository,
			ITeamRepository teamRepository,
			IContractRepository contractRepository,
			IPartTimePercentageRepository partTimePercentageRepository,
			IContractScheduleRepository contractScheduleRepository,
			IPersonRepository personRepository,
			ICurrentUnitOfWork unitOfWork)
		{
			_now = now;
			_siteRepository = siteRepository;
			_teamRepository = teamRepository;
			_contractRepository = contractRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_personRepository = personRepository;
			_unitOfWork = unitOfWork;
		}

		[Route("api/PerformanceTool/Dangerous/ThrowDivideByZeroException"), HttpGet]
		public int ThrowDivideByZeroException()
		{
			var zero = 0;
			var number = 1;
			try
			{
				return number / zero;
			}
			catch (Exception e)
			{
				new RaygunWebApiClient().SendInBackground(e);
				throw;
			}
		}

		[Route("api/PerformanceTool/Dangerous/ThrowException"), HttpGet]
		public void ThrowException()
		{
			var exception = new Exception("Testing");
			new RaygunWebApiClient().SendInBackground(exception);
			throw exception;
		}

		[UnitOfWork, HttpPost, Route("api/PerformanceTool/Dangerous/Create5000Agents")]
		public virtual void Create5000Agents()
		{
			var date = _now.LocalDateOnly();
			var site = new Site("Site");
			_siteRepository.Add(site);
			var team = new Team { Site = site, Description = new Description("team") };
			_teamRepository.Add(team);
			var contract = new Contract("c");
			_contractRepository.Add(contract);
			var partTimePercentage = new PartTimePercentage("p");
			_partTimePercentageRepository.Add(partTimePercentage);
			var contractSchedule = new ContractSchedule("cs");
			_contractScheduleRepository.Add(contractSchedule);

			var utcTimeZone = TimeZoneInfo.Utc;
			for (var i = 0; i < 5000; i++)
			{
				var person = new Person();
				person.PermissionInformation.SetDefaultTimeZone(utcTimeZone);
				var personPeriod = new PersonPeriod(date,
					new PersonContract(contract, partTimePercentage, contractSchedule), team);
				person.AddPersonPeriod(personPeriod);
				_personRepository.Add(person);
			}
			_unitOfWork.Current().PersistAll();
		}
	}
}