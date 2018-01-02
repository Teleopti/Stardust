﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true, false)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true, false)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true, false)]

	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, false, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, false, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true, true)]
	[TestFixture(SeperateWebRequest.SimulateSecondRequestOrScheduler,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryTrue, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest, RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue, true, true)]
	[TestFixture(SeperateWebRequest.SimulateFirstRequest,  RemoveClassicShiftCategory.RemoveClassicShiftCategoryFalse, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse, true, true)]

	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public abstract class SchedulingScenario : IConfigureToggleManager, ITestInterceptor, ISetup
	{
		private readonly SeperateWebRequest _seperateWebRequest;
		protected readonly RemoveClassicShiftCategory ResourcePlannerRemoveClassicShiftCat46582;
		protected readonly RemoveImplicitResCalcContext RemoveImplicitResCalcContext46680;
		protected readonly bool _resourcePlannerTimeZoneIssues45818;
		private readonly bool _resourcePlannerXxl47258;

		public IIoCTestContext IoCTestContext;

		protected SchedulingScenario(SeperateWebRequest seperateWebRequest, 
			RemoveClassicShiftCategory resourcePlannerRemoveClassicShiftCat46582, 
			RemoveImplicitResCalcContext removeImplicitResCalcContext46680,
			bool resourcePlannerTimeZoneIssues45818,
			bool resourcePlannerXxl47258)
		{
			_seperateWebRequest = seperateWebRequest;
			ResourcePlannerRemoveClassicShiftCat46582 = resourcePlannerRemoveClassicShiftCat46582;
			RemoveImplicitResCalcContext46680 = removeImplicitResCalcContext46680;
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
			if(RemoveImplicitResCalcContext46680 == RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
			else
				toggleManager.Disable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
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