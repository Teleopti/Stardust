using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.WebTest.Areas.TeamSchedule;
using Teleopti.Ccc.WebTest.Core.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
    [TestFixture]  
    public class TeamSchedulePersonsProviderTest
    {
        private TeamSchedulePersonsProvider _target;
        private FakeNameFormatSettingProvider _nameFormatSettingProvider;
        private IPersonForScheduleFinder _personForScheduleFinder;

        [SetUp]
        public void Setup()
        {
            var permissionProvider = new FakePermissionProvider();
            _personForScheduleFinder = MockRepository.GenerateMock<IPersonForScheduleFinder>();
            var personRepository = MockRepository.GenerateMock<IPersonRepository>();
            _nameFormatSettingProvider = new FakeNameFormatSettingProvider();

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

            _nameFormatSettingProvider.SetNameFormat(NameFormatSetting.LastNameThenFirstName);
          
            _personForScheduleFinder.Stub(rep => rep.GetPersonFor(input.ScheduleDate, input.TeamIdList, input.SearchNameText, NameFormatSetting.LastNameThenFirstName))
                                            .Return(new List<IAuthorizeOrganisationDetail> ());


            _target.RetrievePeople(input);

            _personForScheduleFinder.AssertWasCalled(rep => rep.GetPersonFor(input.ScheduleDate, input.TeamIdList, input.SearchNameText, NameFormatSetting.LastNameThenFirstName));
            
        }
    }
}
