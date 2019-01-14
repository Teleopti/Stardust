using System;
using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.ExportPages;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.QuickForecast;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SkillPages;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WorkloadPages;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.Forms.PayrollExportPages;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.QuickForecastPages;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{

	public static class PropertyPagesHelper
	{
		public static IList<IPropertyPage> GetSkillPages(bool forWizard,
			 IAbstractPropertyPages abstractPropertyPages, bool hidePriorityToggle, bool showAbandonRate)
		{
			return GetSkillPages(forWizard, abstractPropertyPages, false, hidePriorityToggle, showAbandonRate);
		}

		public static IList<IPropertyPage> BudgetGroupPages()
		{
			return new List<IPropertyPage> { new BudgetGroupGeneral() };
		}

		public static IList<IPropertyPage> GetSkillPages(bool forWizard, IAbstractPropertyPages abstractPropertyPages, bool setEditMode, bool hidePriorityToggle, bool showAbandonRate)
		{
			IList<IPropertyPage> list = new List<IPropertyPage>();
			list.Add(new SkillGeneral(abstractPropertyPages, showAbandonRate));
			list.Add(new SkillThresholds());
			if(!hidePriorityToggle)
				list.Add(new SkillOptimisation());
			if (forWizard)
			{
				list.Add(new SkillDistributions());
			}

			return SetEditMode(list, setEditMode);
		}


		public static IList<IPropertyPage> GetSkillPages(bool forWizard,
			 IAbstractPropertyPages abstractPropertyPages, ISkillType skillType, bool hidePriorityToggle, bool showAbandonRate)
		{
			return GetSkillPages(forWizard, abstractPropertyPages, false, skillType, hidePriorityToggle, showAbandonRate);
		}

		public static IList<IPropertyPage> GetSkillPages(bool forWizard, IAbstractPropertyPages abstractPropertyPages, bool setEditMode, ISkillType skillType, bool hidePriorityToggle, bool showAbandonRate)
		{
			IList<IPropertyPage> list = new List<IPropertyPage>();
			list.Add(new SkillGeneral(abstractPropertyPages, showAbandonRate));
			list.Add(new SkillThresholds());
			if (!hidePriorityToggle)
				list.Add(new SkillOptimisation());
			if (forWizard && !(skillType is SkillTypePhone))
			{
				list.Add(new SkillEmailDistributions());
			}
			else if (forWizard)
			{
				list.Add(new SkillDistributions());
			}

			return SetEditMode(list, setEditMode);
		}

		public static void AddMultisiteSkillPages(IList<IPropertyPage> pages, IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
		{
			pages.Add(
				 new MultisiteChildSkills(staffingCalculatorServiceFacade));
		}

		private static IList<IPropertyPage> SetEditMode(IList<IPropertyPage> list, bool setEditMode)
		{
			if (!setEditMode) return list;
			list.ForEach(l => l.SetEditMode());
			return list;
		}

		public static IList<IPropertyPage> GetWorkloadPages()
		{
			IList<IPropertyPage> list = new List<IPropertyPage>();
			list.Add(new WorkloadGeneral());
			list.Add(new WorkloadQueues());
			list.Add(new WorkloadOpenHours());
			list.Add(new WorkloadQueueAdjustment());

			return list;
		}

		public static IList<IPropertyPage> GetPayrollExportPages(bool showWelcome, IComponentContext componentContext)
		{
			var model = new PersonsSelectionModel();
			IList<IPropertyPage> list = new List<IPropertyPage>();
			if (showWelcome)
			{
				list.Add(new PayrollWelcomePage());
			}
			list.Add(new PayrollExportGeneral());
			list.Add(new FileTypeSelection());
			list.Add(new DateTimePeriodSelection());
			list.Add(new PersonsSelectionView(model, componentContext));
			return list;
		}

		public static IList<IPropertyPage> GetMultisiteSkillDistributionPages()
		{
			return new List<IPropertyPage>
			{
				new MultisiteDistributions()
			};
		}

		public static SelectExportType GetExportSkillFirstPage(Action<bool> pageChange)
		{
			return new SelectExportType(pageChange);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")
		]
		public static IList<IPropertyPageNoRoot<ExportSkillModel>> GetExportAcrossBusinessUnitsPages(
			SelectExportType firstPage)
		{
			return new List<IPropertyPageNoRoot<ExportSkillModel>>
			{
				firstPage,
				new SelectMultisiteSkills(),
				new SelectDestination(),
				new SelectDateRange()
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")
		]
		public static IList<IPropertyPageNoRoot<ExportSkillModel>> GetExportSkillToFilePages(SelectExportType firstPage, IStaffingCalculatorServiceFacade staffingCalculatorService)
		{
			return new List<IPropertyPageNoRoot<ExportSkillModel>>
			{
				firstPage,
				new SelectSkills(),
				new SelectDateAndScenario(),
				new SelectFileDestination(staffingCalculatorService),
				new FileExportFinished()
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")
		]
		public static IList<IPropertyPageNoRoot<ReforecastModelCollection>> GetReforecastFilePages(
			AvailableSkillWithPreselectedSkill skills)
		{
			return new List<IPropertyPageNoRoot<ReforecastModelCollection>>
			{
				new Intraday.Reforecast.SelectSkills(skills),
				new Intraday.Reforecast.SelectWorkload()
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")
		]
		public static IList<IPropertyPageNoRoot<QuickForecastModel>> GetQuickForecastPages(ICollection<ISkill> skills)
		{
			return new List<IPropertyPageNoRoot<QuickForecastModel>>
			{
				new SelectWorkload(skills),
				new SelectHistoricalDateRange(),
				new SelectHistoricalDateRangeForTemplates(),
				new SelectTargetDatesAndScenario()
			};
		}
	}
}
