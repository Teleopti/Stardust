using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    public class SchedulerSkillHelperTest
    {
        private MockRepository _mocks;
        private SchedulerSkillHelper _target;
        private INonBlendSkillFromGroupingCreator _nonBlendSkillFromGroupingCreator;
        private ISchedulerSkillDayHelper _schedulerSkillDayHelper;
        private IGroupPageDataProvider _groupPageDataProvider;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _groupPageDataProvider = _mocks.StrictMock<IGroupPageDataProvider>();
            _nonBlendSkillFromGroupingCreator = _mocks.StrictMock<INonBlendSkillFromGroupingCreator>();
            _schedulerSkillDayHelper = _mocks.StrictMock<ISchedulerSkillDayHelper>();
            _target = new SchedulerSkillHelper(_schedulerSkillDayHelper);
        }

        [Test]
        public void ShouldCreateSkillsAndSkillDaysForNonBlend()
        {
            var groupPage = new GroupPage("GP");
            Expect.Call(_groupPageDataProvider.PersonCollection).Return(new List<IPerson>());
            Expect.Call(() => _nonBlendSkillFromGroupingCreator.ProcessDate(new DateOnly(), groupPage));
            Expect.Call(() => _schedulerSkillDayHelper.AddSkillDaysToStateHolder(ForecastSource.NonBlendSkill, 20));
            _mocks.ReplayAll();
            _target.CreateNonBlendSkillsFromGrouping(_groupPageDataProvider, groupPage, new DateOnly(), _nonBlendSkillFromGroupingCreator, 20);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetBusinessUnitCollectionWhenGroupingIsMain()
        {
            var dataProvider = _mocks.StrictMock<IGroupPageDataProvider>();
            Expect.Call(dataProvider.PersonCollection).Return(new List<IPerson>());
            Expect.Call(dataProvider.BusinessUnitCollection).Return(new List<IBusinessUnit> { new BusinessUnit("BU") });
            _mocks.ReplayAll();
            SchedulerSkillHelper.CreateGroupPageForDate(dataProvider, new GroupPage("Main") { DescriptionKey = "Main" }, new DateOnly());
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetContractCollectionWhenGroupingIsContracts()
        {
            var dataProvider = _mocks.StrictMock<IGroupPageDataProvider>();
            Expect.Call(dataProvider.PersonCollection).Return(new List<IPerson>());
            Expect.Call(dataProvider.ContractCollection).Return(new List<IContract> { new Contract("CO") });
            _mocks.ReplayAll();
            SchedulerSkillHelper.CreateGroupPageForDate(dataProvider, new GroupPage("Contracts") { DescriptionKey = "Contracts" }, new DateOnly());
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetContractScheduleCollectionWhenGroupingIsContractSchedule()
        {
            var dataProvider = _mocks.StrictMock<IGroupPageDataProvider>();
            Expect.Call(dataProvider.PersonCollection).Return(new List<IPerson>());
            Expect.Call(dataProvider.ContractScheduleCollection).Return(new List<IContractSchedule> { new ContractSchedule("CS") });
            _mocks.ReplayAll();
            SchedulerSkillHelper.CreateGroupPageForDate(dataProvider, new GroupPage("ContractSchedule") { DescriptionKey = "ContractSchedule" }, new DateOnly());
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetPartTimePercentagesCollectionWhenGroupingIsPartTimePercentages()
        {
            var dataProvider = _mocks.StrictMock<IGroupPageDataProvider>();
            Expect.Call(dataProvider.PersonCollection).Return(new List<IPerson>());
            Expect.Call(dataProvider.PartTimePercentageCollection).Return(new List<IPartTimePercentage> { new PartTimePercentage("PTP") });
            _mocks.ReplayAll();
            SchedulerSkillHelper.CreateGroupPageForDate(dataProvider, new GroupPage("PartTimepercentages") { DescriptionKey = "PartTimepercentages" }, new DateOnly());
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldOnlyGetPersonCollectionWhenGroupingIsNote()
        {
            var dataProvider = _mocks.StrictMock<IGroupPageDataProvider>();
            Expect.Call(dataProvider.PersonCollection).Return(new List<IPerson>());
            _mocks.ReplayAll();
            var page = SchedulerSkillHelper.CreateGroupPageForDate(dataProvider, new GroupPage("Note") { DescriptionKey = "Note" }, new DateOnly());
            Assert.That(page, Is.Not.Null);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetRuleSetBagCollectionWhenGroupingIsRuleSetBag()
        {
            var dataProvider = _mocks.StrictMock<IGroupPageDataProvider>();
            Expect.Call(dataProvider.PersonCollection).Return(new List<IPerson>());
            Expect.Call(dataProvider.RuleSetBagCollection).Return(new List<IRuleSetBag> { new RuleSetBag { Description = new Description("RSB") } });
            _mocks.ReplayAll();
            var page = SchedulerSkillHelper.CreateGroupPageForDate(dataProvider, new GroupPage("RuleSetBag") { DescriptionKey = "RuleSetBag" }, new DateOnly());
            Assert.That(page, Is.Not.Null);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldOnlyGetPersonCollectionWhenGroupingIsDefault()
        {
            var dataProvider = _mocks.StrictMock<IGroupPageDataProvider>();
            var groupPage = new GroupPage("AnythingUndefined") { DescriptionKey = "AnythingUndefined" };
            Expect.Call(dataProvider.PersonCollection).Return(new List<IPerson>());
            _mocks.ReplayAll();
            var page = SchedulerSkillHelper.CreateGroupPageForDate(dataProvider, groupPage, new DateOnly());
            Assert.That(page, Is.EqualTo(groupPage));
            _mocks.VerifyAll();
        }
    }

    
}