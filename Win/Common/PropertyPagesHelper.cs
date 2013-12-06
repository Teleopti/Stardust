using System;
using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Win.Budgeting;
using Teleopti.Ccc.Win.Forecasting.Forms.ExportPages;
using Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast;
using Teleopti.Ccc.Win.Forecasting.Forms.SkillPages;
using Teleopti.Ccc.Win.Forecasting.Forms.WorkloadPages;
using Teleopti.Ccc.Win.Payroll.Forms.PayrollExportPages;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Ccc.WinCode.Payroll.PayrollExportPages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Helper class for returning the pages to use in property windows and wizards
    /// </summary>
    public static class PropertyPagesHelper
    {
        /// <summary>
        /// Gets the skill pages.
        /// </summary>
        /// <param name="forWizard">if set to <c>true</c> [for wizard].</param>
        /// <param name="abstractPropertyPages">The abstract property pages.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public static IList<IPropertyPage> GetSkillPages(bool forWizard, 
            IAbstractPropertyPages abstractPropertyPages)
        {
            return GetSkillPages(forWizard, abstractPropertyPages, false);
        }

        public static IList<IPropertyPage> BudgetGroupPages()
        {
            return new List<IPropertyPage> {new BudgetGroupGeneral()};
        }

        /// <summary>
        /// Gets the skill pages.
        /// </summary>
        /// <param name="forWizard">if set to <c>true</c> [for wizard].</param>
        /// <param name="abstractPropertyPages">The abstract property pages.</param>
        /// <param name="setEditMode">if set to <c>true</c> [set edit mode].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-25
        /// </remarks>
        public static IList<IPropertyPage> GetSkillPages(bool forWizard, IAbstractPropertyPages abstractPropertyPages, bool setEditMode)
        {
            IList<IPropertyPage> list = new List<IPropertyPage>();
            list.Add(new SkillGeneral(abstractPropertyPages));
            list.Add(new SkillThresholds());
            list.Add(new SkillOptimisation());
            if (forWizard)
            {
                list.Add(new SkillDistributions());
            }

            return SetEditMode(list,setEditMode);
        }


        public static IList<IPropertyPage> GetSkillPages(bool forWizard,
            IAbstractPropertyPages abstractPropertyPages, ISkillType skillType)
        {
            return GetSkillPages(forWizard, abstractPropertyPages, false, skillType);
        }

        /// <summary>
        /// Gets the skill pages.
        /// </summary>
        /// <param name="forWizard">if set to <c>true</c> [for wizard].</param>
        /// <param name="abstractPropertyPages">The abstract property pages.</param>
        /// <param name="setEditMode">if set to <c>true</c> [set edit mode].</param>
        /// <param name="skillType">Type of the skill.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-25
        /// </remarks>
        public static IList<IPropertyPage> GetSkillPages(bool forWizard, IAbstractPropertyPages abstractPropertyPages, bool setEditMode, ISkillType skillType)
        {
            IList<IPropertyPage> list = new List<IPropertyPage>();
            list.Add(new SkillGeneral(abstractPropertyPages));
            list.Add(new SkillThresholds());
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

        /// <summary>
        /// Adds the multisite skill pages.
        /// </summary>
        /// <param name="pages">The pages.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-25
        /// </remarks>
        public static void AddMultisiteSkillPages(IList<IPropertyPage> pages)
        {
            pages.Add(
                new MultisiteChildSkills());
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="setEditMode">if set to <c>true</c> [set edit mode].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-23
        /// </remarks>
        private static IList<IPropertyPage> SetEditMode(IList<IPropertyPage> list, bool setEditMode)
        {
            if (!setEditMode) return list;
            list.ForEach(l => l.SetEditMode());
            return list;
        }

        /// <summary>
        /// Gets the workload pages.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public static IList<IPropertyPage> GetWorkloadPages()
        {
            IList<IPropertyPage> list = new List<IPropertyPage>();
            list.Add(new WorkloadGeneral());
            list.Add(new WorkloadQueues());
            list.Add(new WorkloadOpenHours());
            list.Add(new WorkloadQueueAdjustment());

            return list;
        }


        /// <summary>
        /// Gets the payroll export pages.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-02-24
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static IList<IPropertyPage> GetPayrollExportPages(bool showWelcome,IComponentContext componentContext )
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

        /// <summary>
        /// Gets the multisite skill distribution page.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-23
        /// </remarks>
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IList<IPropertyPageNoRoot<ExportSkillModel>> GetExportAcrossBusinessUnitsPages(SelectExportType firstPage)
        {
            return new List<IPropertyPageNoRoot<ExportSkillModel>>
                       {
                           firstPage,
                           new SelectMultisiteSkills(),
                           new SelectDestination(),
                           new SelectDateRange()
                       };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IList<IPropertyPageNoRoot<ExportSkillModel>> GetExportSkillToFilePages(SelectExportType firstPage)
        {
            return new List<IPropertyPageNoRoot<ExportSkillModel>>
                       {
                           firstPage,
                           new SelectSkills(),
                           new SelectDateAndScenario(),
                           new SelectFileDestination(),
                           new FileExportFinished()
                       };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IList<IPropertyPageNoRoot<ReforecastModelCollection>> GetReforecastFilePages(AvailableSkillWithPreselectedSkill skills)
        {
            return new List<IPropertyPageNoRoot<ReforecastModelCollection>>
                       {
                           new Intraday.Reforecast.SelectSkills(skills),
                           new Intraday.Reforecast.SelectWorkload()
                       };
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IList<IPropertyPageNoRoot<QuickForecastCommandDto>> GetQuickForecastPages(ICollection<ISkill> skills)
		{
			return new List<IPropertyPageNoRoot<QuickForecastCommandDto>>
                       {
                           new SelectWorkload(skills),
						   new SelectHistoricalDateRange(),
						   new SelectHistoricalDateRangeForTemplates(),
						   new SelectTargetDatesAndScenario()
                       };
		}
    }
}
