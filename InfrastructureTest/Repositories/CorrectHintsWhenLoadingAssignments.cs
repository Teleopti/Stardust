using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture(true)]
	[TestFixture(false)]
	[InfrastructureTest]
	[Ignore("#79780 To be fixed")]
	public class CorrectHintsWhenLoadingAssignments : IConfigureToggleManager
	{
		private readonly bool _toggle;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;

		public CorrectHintsWhenLoadingAssignments(bool toggle)
		{
			_toggle = toggle;
		}
		
		[Test]
		public void ShouldForceSeekHintOnLayersWhenLoadingAssignments()
		{
			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (var spy = new SqlSpy())
				{
					PersonAssignmentRepository.Find(new[]{new Person().WithId(), },
						DateOnly.Today.ToDateOnlyPeriod(), new Scenario().WithId());

					var log = spy.WholeLog();
					log.Should().Not.Be.Empty();
					//how reliable and "static" is "shiftlayer2_"? Better to use some regex here...
					var queryContainsHint = log.Contains("left outer join dbo.ShiftLayer shiftlayer2_ WITH (FORCESEEK)");
					queryContainsHint.Should().Be.EqualTo(_toggle);
				}				
			}
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_toggle)
			{
				toggleManager.Enable(Toggles.ResourcePlanner_QueryHintOnLayers_79780);
			}
		}
	}
}