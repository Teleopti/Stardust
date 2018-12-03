using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation.IntraIntervalAnalyze
{
	[TestFixture]
	public class IntraIntervalFinderTest
	{
		private MockRepository _mocks;
		private IntraIntervalFinder _target;
		private DateTimePeriod _interval;
		private IResourceCalculationDataContainerWithSingleOperation _resourceCalculationDataContainer;
		private ISkill _skill;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_resourceCalculationDataContainer = _mocks.StrictMock<IResourceCalculationDataContainerWithSingleOperation>();
			_interval = new DateTimePeriod();
			_skill = SkillFactory.CreateSkill("skill");
			_target = new IntraIntervalFinder();
		}

		[Test]
		public void ShouldReturnAnyIfFound()
		{
			using (_mocks.Record())
			{
				Expect.Call(_resourceCalculationDataContainer.IntraIntervalResources(_skill, _interval)).Return(new List<DateTimePeriod> {new DateTimePeriod()});
			}

			using (_mocks.Playback())
			{
				var result = _target.FindForInterval(_interval, _resourceCalculationDataContainer, _skill);
				Assert.IsTrue(result.Any());
			}
		}

		[Test]
		public void ShouldNoneIfNotFound()
		{
			using (_mocks.Record())
			{
				Expect.Call(_resourceCalculationDataContainer.IntraIntervalResources(_skill, _interval)).Return(new List<DateTimePeriod>());
			}

			using (_mocks.Playback())
			{
				var result = _target.FindForInterval(_interval, _resourceCalculationDataContainer, _skill);
				Assert.IsFalse(result.Any());
			}
		}
	}
}
