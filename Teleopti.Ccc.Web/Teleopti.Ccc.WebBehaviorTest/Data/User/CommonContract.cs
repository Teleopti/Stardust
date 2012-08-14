using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class CommonContract : IDataSetup, IContractSetup
	{
		public IContract Contract { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			Contract = ContractFactory.CreateContract("Common contract");
			new ContractRepository(uow).Add(Contract);
		}
	}

	public class CommonSite : IDataSetup
	{
		public ISite Site;

		public void Apply(IUnitOfWork uow)
		{
			var businessUnit = GlobalDataContext.Data().Data<CommonBusinessUnit>().BusinessUnit;

			Site = SiteFactory.CreateSimpleSite("Common Site");
			var siteRepository = new SiteRepository(uow);
			siteRepository.Add(Site);
			businessUnit.AddSite(Site);
		}
	}
	
	public class AnotherSite : IDataSetup
	{
		public ISite Site;

		public void Apply(IUnitOfWork uow)
		{
			var businessUnit = GlobalDataContext.Data().Data<CommonBusinessUnit>().BusinessUnit;

			Site = SiteFactory.CreateSimpleSite("Another Site");
			var siteRepository = new SiteRepository(uow);
			siteRepository.Add(Site);
			businessUnit.AddSite(Site);
		}
	}

	public class CommonBusinessUnit : IDataSetup
	{
		public static IBusinessUnit BusinessUnitFromFakeState;

		public IBusinessUnit BusinessUnit { get { return BusinessUnitFromFakeState; } }

		public void Apply(IUnitOfWork uow)
		{
			var businessUnitRepository = new BusinessUnitRepository(uow);
			businessUnitRepository.Add(BusinessUnit);
		}
	}

	public class SecondBusinessUnit : IDataSetup
	{
		public IBusinessUnit BusinessUnit;

		public void Apply(IUnitOfWork uow)
		{
			BusinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("SecondBusinessUnit");
			var businessUnitRepository = new BusinessUnitRepository(uow);
			businessUnitRepository.Add(BusinessUnit);
		}
	}

	public class CommonTeam : IDataSetup
	{
		public ITeam Team;

		public void Apply(IUnitOfWork uow)
		{
			var site = GlobalDataContext.Data().Data<CommonSite>().Site;

			Team = TeamFactory.CreateSimpleTeam("Common Team");
			site.AddTeam(Team);

			var teamRepository = new TeamRepository(uow);
			teamRepository.Add(Team);
		}
	}
}