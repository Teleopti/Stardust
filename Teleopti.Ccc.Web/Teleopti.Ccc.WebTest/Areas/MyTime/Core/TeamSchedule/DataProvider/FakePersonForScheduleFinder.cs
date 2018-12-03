using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Core;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	class FakePersonForScheduleFinder : IPersonForScheduleFinder
	{
		private IPersonRepository _personRepository;
		private IPersonNameProvider _personNameProvider;
		private IBusinessUnitRepository _businessUnitRepository;
		

		public FakePersonForScheduleFinder(IPersonRepository personRepository, IBusinessUnitRepository businessUnitRepository, IPersonNameProvider personNameProvider)
		{
			_personRepository = personRepository;
			_businessUnitRepository = businessUnitRepository;
			_personNameProvider = personNameProvider;
		}

		public IList<IPersonAuthorization> GetPersonFor(DateOnly shiftTradeDate, IList<Guid> teamIdList, string name, NameFormatSetting nameFormat = NameFormatSetting.FirstNameThenLastName)
		{
			var businessUnit = _businessUnitRepository.LoadAll().FirstOrDefault();
			name = name ?? string.Empty;
			var personsOrganisationDetail = _personRepository.LoadAll().Where(x =>
			{
				var period = new DateOnlyPeriod(shiftTradeDate, shiftTradeDate);

				return x.PersonPeriods(period).Any(p => teamIdList.Contains(p.Team.Id.GetValueOrDefault()))
					 && _personNameProvider.BuildNameFromSetting(x.Name).Contains(name);


			}).Select<IPerson, IPersonAuthorization>(x => new PersonSelectorShiftTrade
			{
				PersonId = x.Id.GetValueOrDefault(),
				TeamId = x.MyTeam(shiftTradeDate).Id,
				SiteId = null,
				BusinessUnitId = businessUnit != null? businessUnit.Id.GetValueOrDefault() : Guid.Empty

			}).ToList();

			return personsOrganisationDetail;

		}
	}
}
