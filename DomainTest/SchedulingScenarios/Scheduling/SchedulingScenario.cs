using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(SeperateWebRequest.DoSeperateWebRequest, EasierBlockScheduling.DoEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(SeperateWebRequest.DoSeperateWebRequest, EasierBlockScheduling.DontEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(SeperateWebRequest.DoSeperateWebRequest, EasierBlockScheduling.DoEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(SeperateWebRequest.DoSeperateWebRequest,  EasierBlockScheduling.DontEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(SeperateWebRequest.DontSeperateWebRequest, EasierBlockScheduling.DoEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(SeperateWebRequest.DontSeperateWebRequest,  EasierBlockScheduling.DontEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(SeperateWebRequest.DontSeperateWebRequest, EasierBlockScheduling.DoEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(SeperateWebRequest.DontSeperateWebRequest,  EasierBlockScheduling.DontEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(SeperateWebRequest.DoSeperateWebRequest, EasierBlockScheduling.DoEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[TestFixture(SeperateWebRequest.DoSeperateWebRequest, EasierBlockScheduling.DontEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[TestFixture(SeperateWebRequest.DoSeperateWebRequest, EasierBlockScheduling.DoEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[TestFixture(SeperateWebRequest.DoSeperateWebRequest,  EasierBlockScheduling.DontEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[TestFixture(SeperateWebRequest.DontSeperateWebRequest, EasierBlockScheduling.DoEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[TestFixture(SeperateWebRequest.DontSeperateWebRequest,  EasierBlockScheduling.DontEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[TestFixture(SeperateWebRequest.DontSeperateWebRequest, EasierBlockScheduling.DoEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[TestFixture(SeperateWebRequest.DontSeperateWebRequest,  EasierBlockScheduling.DontEasierBlockScheduling, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public abstract class SchedulingScenario : IConfigureToggleManager, ITestInterceptor
	{
		protected readonly SeperateWebRequest SeperateWebRequest;
		protected readonly EasierBlockScheduling ResourcePlannerEasierBlockScheduling46155;
		protected readonly RemoveClassicShiftCategory ResourcePlannerRemoveClassicShiftCat46582;
		protected readonly RemoveImplicitResCalcContext RemoveImplicitResCalcContext46680;
		
		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(SeperateWebRequest seperateWebRequest, EasierBlockScheduling resourcePlannerEasierBlockScheduling46155, RemoveClassicShiftCategory resourcePlannerRemoveClassicShiftCat46582, RemoveImplicitResCalcContext removeImplicitResCalcContext46680)
		{
			SeperateWebRequest = seperateWebRequest;
			ResourcePlannerEasierBlockScheduling46155 = resourcePlannerEasierBlockScheduling46155;
			ResourcePlannerRemoveClassicShiftCat46582 = resourcePlannerRemoveClassicShiftCat46582;
			RemoveImplicitResCalcContext46680 = removeImplicitResCalcContext46680;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(ResourcePlannerEasierBlockScheduling46155 == EasierBlockScheduling.DoEasierBlockScheduling)
				toggleManager.Enable(Toggles.ResourcePlanner_EasierBlockScheduling_46155);
			if(ResourcePlannerRemoveClassicShiftCat46582 == RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveClassicShiftCat_46582);
			if(RemoveImplicitResCalcContext46680 == RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
			else
				toggleManager.Disable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
		}

		public void OnBefore()
		{
			if (SeperateWebRequest == SeperateWebRequest.DoSeperateWebRequest)
				IoCTestContext.SimulateNewRequest();
		}
	}
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_EasierBlockScheduling_46155)]
	public enum EasierBlockScheduling
	{
		DoEasierBlockScheduling,
		DontEasierBlockScheduling
	}
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveClassicShiftCat_46582)]
	public enum RemoveClassicShiftCategory
	{
		RemoveClassicShiftCategoryTrue,
		RemoveClassicShiftCategoryFalse
	}
}