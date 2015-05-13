using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.Mapping
{
	[TestFixture]
	[TeamScheduleTestAttribute]
	public class TeamScheduleViewModelReworkedMapperTest
	{
		public ITeamScheduleViewModelReworkedMapper Mapper;
		
		[Test]
		public void ShouldMap()
		{
			var target = Mapper.Map(new TeamScheduleViewModelData());
			target.Should().Not.Be.Null();
		}
	}

	public class FakePersonScheduleDayReadModelFinder : IPersonScheduleDayReadModelFinder
	{
		public IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnly startDate, DateOnly endDate, Guid personId)
		{
			throw new NotImplementedException();
		}

		public PersonScheduleDayReadModel ForPerson(DateOnly date, Guid personId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPeople(DateTimePeriod period, IEnumerable<Guid> personIds)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForBulletinPersons(IEnumerable<string> shiftExchangeOfferIdList, Paging paging)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPersons(DateOnly date, IEnumerable<Guid> personIdList, Paging paging, TimeFilterInfo filterInfo = null,
			string timeSortOrder = "")
		{
			throw new NotImplementedException();
		}
	}

	public class FakeTeamSchedulePersonsProvider : ITeamSchedulePersonsProvider
	{
		public IEnumerable<Guid> RetrievePersons(TeamScheduleViewModelData data)
		{
			throw new NotImplementedException();
		}
	}
}
