using System;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
	[LocalHostAccess]
	public class DangerousController : Controller
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

		[UnitOfWork]
		public virtual void Create5000Agents()
		{
			var date = new DateOnly(_now.UtcDateTime());
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
			for (var i = 0; i < 5000; i++)
			{
				var person = new Person();
				person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
				var personPeriod = new PersonPeriod(date,
					new PersonContract(contract, partTimePercentage, contractSchedule), team);
				person.AddPersonPeriod(personPeriod);
				_personRepository.Add(person);
			}
			_unitOfWork.Current().PersistAll();
		}

		public int ThrowDivideByZeroException()
		{
			var zero = 0;
			var number = 1;
			return number/zero;
		}

		public void ThrowException()
		{
			throw new Exception("Testing");
		}
	}

	public class LocalHostAccessAttribute : AuthorizeAttribute
	{
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			if (httpContext.Request.IsLocal)
				return true;
			throw new HttpException(404, "");
		}
	}
}