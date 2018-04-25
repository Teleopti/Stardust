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
	public class ImportJobArtifactValidatorTest : IIsolateSystem
	{
		public IImportJobArtifactValidator Target;
		public IStardustJobFeedback Feedback;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ImportJobArtifactValidator>().For<IImportJobArtifactValidator>();
			isolate.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
		}

		[Test]
		public void ShouldReturnNullWhenCannotFindTheJob()
		{
			var result = Target.ValidateJobArtifact(null, Feedback.SendProgress);

			result.Should().Be.Null();
		}
	}
}
