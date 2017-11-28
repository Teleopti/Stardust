using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture, DomainTest]
	public class ImportJobArtifactValidatorTest : ISetup
	{
		public IImportJobArtifactValidator Target;
		public IStardustJobFeedback Feedback;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ImportJobArtifactValidator>().For<IImportJobArtifactValidator>();
			system.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindTheJob()
		{
			var result = Target.ValidateJobArtifact(null, Feedback.SendProgress);

			result.Should().Be.Null();
		}
	}
}
