using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.PerformanceTool;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.PerformanceTool
{
	[Category("LongRunning")]
	[TestFixture]
	public class StateGeneratorTest : DatabaseTest
	{
		[Test]
		public void ShouldGenerateAStateGroup()
		{
			var unitOfWork = new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()));
			var rtaStateGroupRepository = new RtaStateGroupRepository(unitOfWork);
			var target = new StateGenerator(unitOfWork, rtaStateGroupRepository);
			target.Generate(1);
			var stateGroups = rtaStateGroupRepository.LoadAll();
			stateGroups.Count.Should().Be(1);
		}

		[Test]
		public void ShouldGenerateStateGroups()
		{
			var unitOfWork = new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()));
			var rtaStateGroupRepository = new RtaStateGroupRepository(unitOfWork);
			var target = new StateGenerator(unitOfWork, rtaStateGroupRepository);
			target.Generate(2);
			var stateGroups = rtaStateGroupRepository.LoadAll();
			stateGroups.Count.Should().Be(2);
		}

		[Test]
		public void ShouldReturnAllGeneratedStateCodes()
		{
			var unitOfWork = new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()));
			var rtaStateGroupRepository = new RtaStateGroupRepository(unitOfWork);
			var target = new StateGenerator(unitOfWork, rtaStateGroupRepository);
			var stateCodes = target.Generate(1);
			var stateGroups = rtaStateGroupRepository.LoadAll();
			stateCodes.Single().Should().Be(stateGroups.Single().StateCollection.Single().StateCode);
		}
	}
}
