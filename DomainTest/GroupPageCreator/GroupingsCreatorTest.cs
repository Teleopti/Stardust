using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.GroupPageCreator
{
	[TestFixture]
	public class GroupingsCreatorTest
	{
		private GroupingsCreator _target;
		private MockRepository mocks;
		private IToggleManager _toggleManager;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			_toggleManager = mocks.Stub<IToggleManager>();
		}

		[Test]
		public void VerifyCreationOfBuiltInGroupings()
		{		
			IGroupPageDataProvider groupingsDataProvider = mocks.StrictMock<IGroupPageDataProvider>();
			using (mocks.Record())
			{
				Expect.Call(_toggleManager.IsEnabled(Toggles.Reporting_Optional_Columns_42066))
				.Return(false);
				Expect.Call(groupingsDataProvider.BusinessUnit).Return(BusinessUnitFactory.BusinessUnitUsedInTest);
				Expect.Call(groupingsDataProvider.ContractCollection).Return(new List<IContract>());
				Expect.Call(groupingsDataProvider.ContractScheduleCollection).Return(new List<IContractSchedule>());
				Expect.Call(groupingsDataProvider.PartTimePercentageCollection).Return(new List<IPartTimePercentage>());
				Expect.Call(groupingsDataProvider.PersonCollection).Return(new List<IPerson>()).Repeat.AtLeastOnce();
				Expect.Call(groupingsDataProvider.RuleSetBagCollection).Return(new List<IRuleSetBag>());
				Expect.Call(groupingsDataProvider.SelectedPeriod).Return(new DateOnlyPeriod());
				Expect.Call(groupingsDataProvider.SkillCollection).Return(new List<ISkill>());
			}
			using (mocks.Playback())
			{
				_target = new GroupingsCreator(groupingsDataProvider, _toggleManager);

				IList<IGroupPage> groupPages = _target.CreateBuiltInGroupPages(true);

				Assert.AreEqual(7, groupPages.Count);
				Assert.AreEqual(groupPages[0].DescriptionKey, "Main");
				Assert.AreEqual(groupPages[1].DescriptionKey, "Contracts");
				Assert.AreEqual(groupPages[2].DescriptionKey, "ContractSchedule");
				Assert.AreEqual(groupPages[3].DescriptionKey, "PartTimepercentages");
				Assert.AreEqual(groupPages[4].DescriptionKey, "Note");
				Assert.AreEqual(groupPages[5].DescriptionKey, "RuleSetBag");
				Assert.AreEqual(groupPages[6].DescriptionKey, "Skill");
			}
		}
	}
}
