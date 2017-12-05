using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequestOrScheduler,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequestOrScheduler,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequestOrScheduler,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequestOrScheduler,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public abstract class SchedulingScenario : IConfigureToggleManager, ITestInterceptor, ISetup
	{
		protected readonly SeperateWebRequest SeperateWebRequest;
		protected readonly RemoveClassicShiftCategory ResourcePlannerRemoveClassicShiftCat46582;
		protected readonly RemoveImplicitResCalcContext RemoveImplicitResCalcContext46680;
		private readonly bool _resourcePlannerTimeZoneIssues45818;

		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(SeperateWebRequest seperateWebRequest, 
			RemoveClassicShiftCategory resourcePlannerRemoveClassicShiftCat46582, 
			RemoveImplicitResCalcContext removeImplicitResCalcContext46680,
			bool resourcePlannerTimeZoneIssues45818)
		{
			SeperateWebRequest = seperateWebRequest;
			ResourcePlannerRemoveClassicShiftCat46582 = resourcePlannerRemoveClassicShiftCat46582;
			RemoveImplicitResCalcContext46680 = removeImplicitResCalcContext46680;
			_resourcePlannerTimeZoneIssues45818 = resourcePlannerTimeZoneIssues45818;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(ResourcePlannerRemoveClassicShiftCat46582 == RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveClassicShiftCat_46582);
			if(_resourcePlannerTimeZoneIssues45818)
				toggleManager.Enable(Toggles.ResourcePlanner_TimeZoneIssues_45818);
			if(RemoveImplicitResCalcContext46680 == RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
			else
				toggleManager.Disable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
		}

		public void OnBefore()
		{
			if (SeperateWebRequest == SeperateWebRequest.SimulateSecondRequestOrScheduler)
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