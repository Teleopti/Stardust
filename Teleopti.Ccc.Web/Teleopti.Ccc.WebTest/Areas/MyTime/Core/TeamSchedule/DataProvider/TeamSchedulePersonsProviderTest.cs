using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
    [TestFixture]  
    public class TeamSchedulePersonsProviderTest
    {
        private TeamSchedulePersonsProvider _target;
        private ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettingProvider;
        private IPersonForScheduleFinder _personForScheduleFinder;

        [SetUp]
        public void Setup()
        {
            var permissionProvider = new FakePermissionProvider();
			var personRepository = new FakePersonRepository(null);

	        _nameFormatSettingProvider = new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository());
			_personForScheduleFinder = MockRepository.GenerateMock<IPersonForScheduleFinder>();
            
            _target = new TeamSchedulePersonsProvider(permissionProvider, _personForScheduleFinder, personRepository, _nameFormatSettingProvider);           
        }


        [Test]
        public void RetrievePersonBySearchNameAndNameFormat()
        {
            var input = new TeamScheduleViewModelData
            {
                ScheduleDate = new DateOnly(),
                TeamIdList = new List<Guid>(),
                SearchNameText = "aa"
            };

            _nameFormatSettingProvider.Persist(new NameFormatSettings{NameFormatId = (int)NameFormatSetting.LastNameThenFirstName});
          
            _personForScheduleFinder.Stub(rep => rep.GetPersonFor(input.ScheduleDate, input.TeamIdList, input.SearchNameText, NameFormatSetting.LastNameThenFirstName))
                                            .Return(new List<IPersonAuthorization> ());


            _target.RetrievePeople(input);

            _personForScheduleFinder.AssertWasCalled(rep => rep.GetPersonFor(input.ScheduleDate, input.TeamIdList, input.SearchNameText, NameFormatSetting.LastNameThenFirstName));
            
        }
    }
}
