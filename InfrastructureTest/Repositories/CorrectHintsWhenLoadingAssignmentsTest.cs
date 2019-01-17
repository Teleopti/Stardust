using System.Text.RegularExpressions;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture(true)]
	[TestFixture(false)]
	[InfrastructureTest]
	public class CorrectHintsWhenLoadingAssignmentsTest : IConfigureToggleManager
	{
		private readonly bool _toggle;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;

		public CorrectHintsWhenLoadingAssignmentsTest(bool toggle)
		{
			_toggle = toggle;
		}
		
		[Test]
		public void ShouldForceSeekHintOnLayersWhenLoadingAssignments()
		{
			const string pattern = @"left outer join dbo.ShiftLayer\s+(\S+)\s+WITH \(FORCESEEK\)";
			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (var spy = new SqlSpy())
				{
					PersonAssignmentRepository.Find(new[]{new Person().WithId(), },
						DateOnly.Today.ToDateOnlyPeriod(), new Scenario().WithId());

					var log = spy.WholeLog();
					log.Should().Not.Be.Empty();
					var queryContainsHint = new Regex(pattern).Match(log).Success;
					queryContainsHint.Should().Be.EqualTo(_toggle);
				}				
			}
		}
		
		[Test]
		public void ShouldNotCreateHintByDefault()
		{
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				using (var spy = new SqlSpy())
				{
					uow.FetchSession().CreateCriteria<PersonAssignment>()
						.Fetch("ShiftLayers")
						.List();
					var log = spy.WholeLog();
					log.Should().Not.Be.Empty();
					log.Should().Not.Contain("WITH (");
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