using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.InfrastructureTest.Toggle
{
	public class ApplicationFunctionFeatureFlagTest
	{
		[Test]
		public void ShouldExcludeFunctionsBasedOnToggleForMyReportAgentQueueMetrics()
		{
			var toggleManager = MockRepository.GenerateMock<IToggleManager>();
			var provider = MockRepository.GenerateMock<IApplicationFunctionsProvider>();

			toggleManager.Stub(x => x.IsEnabled(Toggles.MyReport_AgentQueueMetrics_22254)).Return(false);
			provider.Stub(x => x.AllFunctions())
				.Return(
					new AllFunctions(new Collection<SystemFunction>
					{
						new SystemFunction(
							new ApplicationFunction(DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics)
							{
								ForeignId = DefinedRaptorApplicationFunctionForeignIds.MyReportQueueMetrics
							}, _ => true)
					}));

			var filterFunctionsBasedOnToggle = new ApplicationFunctionsToggleFilter(provider, toggleManager);
			var result = filterFunctionsBasedOnToggle.FilteredFunctions();

			result.Functions.Single().Hidden.Should().Be.True();
		}
	}
}