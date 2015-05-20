﻿using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

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

		public IList<IAuthorizeOrganisationDetail> GetPersonFor(DateOnly shiftTradeDate, IList<Guid> teamIdList, string name)
		{
			var businessUnit = _businessUnitRepository.LoadAll().First();


			var personsOrganisationDetail = _personRepository.LoadAll().Where(x =>
			{
				var period = new DateOnlyPeriod(shiftTradeDate, shiftTradeDate);
				return x.PersonPeriods(period).Any(p => teamIdList.Contains(p.Team.Id.Value))
				       && _personNameProvider.BuildNameFromSetting(x.Name).Contains(name);
			}).Select<IPerson,IAuthorizeOrganisationDetail>(x => new PersonSelectorShiftTrade
			{
				PersonId = x.Id.Value,
				TeamId = x.MyTeam(shiftTradeDate).Id,
				SiteId = null,
				BusinessUnitId = businessUnit.Id.Value

			}).ToList();

			return personsOrganisationDetail;

		}
	}
}
