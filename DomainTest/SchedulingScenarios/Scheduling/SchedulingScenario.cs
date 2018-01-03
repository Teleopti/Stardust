using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, false, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, false, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, true, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, true, false)]

	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, false, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, false, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, true, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, true, true)]

	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public abstract class SchedulingScenario : IConfigureToggleManager, ITestInterceptor, ISetup
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		protected readonly RemoveClassicShiftCategory ResourcePlannerRemoveClassicShiftCat46582;
		protected readonly bool _resourcePlannerTimeZoneIssues45818;
		private readonly bool _resourcePlannerXxl47258;

		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(SeperateWebRequest seperateWebRequest, 
			RemoveClassicShiftCategory resourcePlannerRemoveClassicShiftCat46582,
			bool resourcePlannerTimeZoneIssues45818,
			bool resourcePlannerXxl47258)
		{
			_seperateWebRequest = seperateWebRequest;
			ResourcePlannerRemoveClassicShiftCat46582 = resourcePlannerRemoveClassicShiftCat46582;
			_resourcePlannerTimeZoneIssues45818 = resourcePlannerTimeZoneIssues45818;
			_resourcePlannerXxl47258 = resourcePlannerXxl47258;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if (_resourcePlannerXxl47258)
				toggleManager.Enable(Toggles.ResourcePlanner_XXL_47258);
			if(ResourcePlannerRemoveClassicShiftCat46582 == RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveClassicShiftCat_46582);
			if(_resourcePlannerTimeZoneIssues45818)
				toggleManager.Enable(Toggles.ResourcePlanner_TimeZoneIssues_45818);
		}

		public void OnBefore()
		{
			if (_seperateWebRequest == SeperateWebRequest.SimulateSecondRequestOrScheduler)
				IoCTestContext.SimulateNewRequest();
		}

		public virtual void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<ResourceCalculateWithNewContext>();
		}
	}
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveClassicShiftCat_46582)]
	public enum RemoveClassicShiftCategory
	{
		RemoveClassicShiftCategoryTrue,
		RemoveClassicShiftCategoryFalse
	}
}