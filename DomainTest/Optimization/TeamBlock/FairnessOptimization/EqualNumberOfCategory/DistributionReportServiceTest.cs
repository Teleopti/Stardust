using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	[TestFixture]
	public class DistributionReportServiceTest
	{
		private MockRepository _mocks;
		private IDistributionReportService _target;
		private IDistributionForPersons _distributionForPersons;
		private IGroupCreator _groupCreator;
		private IPerson _person1;
		private IPerson _person2;
		private List<IPerson> _allPersons;
		private IGroupPage _groupPageForDate;
		private IScheduleDictionary _scheduleDictionary;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_distributionForPersons = _mocks.StrictMock<IDistributionForPersons>();
			_groupCreator = _mocks.StrictMock<IGroupCreator>();
			_target = new DistributionReportService(_distributionForPersons, _groupCreator);
			_person1 = PersonFactory.CreatePerson();
			_person2 = PersonFactory.CreatePerson();
			_allPersons = new List<IPerson> {_person1, _person2};
			_groupPageForDate = _mocks.StrictMock<IGroupPage>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
		}

		[Test]
		public void ShouldCreateReport()
		{
			var wfcs = new WorkflowControlSet();
			wfcs.SetFairnessType(FairnessType.EqualNumberOfShiftCategory);
			_person1.WorkflowControlSet = wfcs;
			var category1 = new ShiftCategory("hej");
			var distributionDic = new Dictionary<IShiftCategory, int>();
			distributionDic.Add(category1, 1);
			var distributionSummary = new DistributionSummary(distributionDic);
			var group = new Group();
			group.AddMember(_person1);
			var scheduleDicKeys = new List<IPerson> {_person1};

			using (_mocks.Record())
			{
				Expect.Call(_distributionForPersons.CreateSummary(new List<IPerson> { _person1 }, _scheduleDictionary))
				      .Return(distributionSummary);
				Expect.Call(_distributionForPersons.CreateSummary(new List<IPerson> { _person1 }, _scheduleDictionary))
					  .Return(distributionSummary);
				Expect.Call(_scheduleDictionary.Keys).Return(scheduleDicKeys);
				Expect.Call(_groupCreator.CreateGroupForPerson(Arg<IPerson>.Is.Equal(_person1), Arg<IGroupPage>.Is.Equal(_groupPageForDate), Arg<HashSet<IPerson>>.List.ContainsAll(scheduleDicKeys))).Return(group);
				Expect.Call(_distributionForPersons.CreateSummary(group.GroupMembers, _scheduleDictionary))
				      .Return(distributionSummary);
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateReport(_person1, _groupPageForDate, _allPersons, _scheduleDictionary);
				Assert.That(result.DistributionDictionary[category1].Agent.Equals(1));
				Assert.That(result.DistributionDictionary[category1].Agent.Equals(result.DistributionDictionary[category1].Team));
				Assert.That(result.DistributionDictionary[category1].Team.Equals(result.DistributionDictionary[category1].All));
			}
		}

		//I have a total of 4 agents 2 of them are in one team. Only one day is scheduled, and with different categories for each agent

	}
}