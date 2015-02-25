using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[UnitOfWorkTest]
	public class GetStateCodeInfosTest
	{
		public IDatabaseWriter Writer;
		public IDatabaseReader Target;
		public IRtaStateGroupRepository StateGroupRepository;
		public ICurrentUnitOfWork UnitOfWork;

		[Test]
		public void ShouldGetStateCodeInfos()
		{
			var stategroup = new RtaStateGroup(" ", true, true);
			StateGroupRepository.Add(stategroup);
			UnitOfWork.Current().PersistAll();

			Writer.AddAndGetStateCode("statecode", "statedescription", Guid.NewGuid(), stategroup.BusinessUnit.Id.Value);

			var result = Target.StateCodeInfos();

			result.Single().StateCode.Should().Be("STATECODE");
		}
	}
}