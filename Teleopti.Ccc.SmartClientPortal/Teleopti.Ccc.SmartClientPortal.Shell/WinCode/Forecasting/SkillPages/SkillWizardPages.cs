using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.SkillPages
{
    /// <summary>
    /// Page manager class for skill wizard pages
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-16
    /// </remarks>
    public class SkillWizardPages : AbstractWizardPages<ISkill>
    {
        private ISkillType _preselectedSkillType;
		private readonly IStaffingCalculatorServiceFacade _staffingCalculatorServiceFacade;

		public SkillWizardPages(ISkillType preselectedSkillType, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory, IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
            : base(repositoryFactory,unitOfWorkFactory)
		{
			_preselectedSkillType = preselectedSkillType;
			_staffingCalculatorServiceFacade = staffingCalculatorServiceFacade;
		}

        public SkillWizardPages(ISkill skill, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory, IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
            : base(skill,repositoryFactory,unitOfWorkFactory)
        {
			_staffingCalculatorServiceFacade = staffingCalculatorServiceFacade;
		}

        /// <summary>
        /// Gets the repository object.
        /// </summary>
        /// <value>The repository object.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public override IRepository<ISkill> RepositoryObject
        {
            get
            {
                return RepositoryFactory.CreateSkillRepository(UnitOfWork);
            }
        }

		/// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public override string Name
        {
            get
            {
                ISkill s = AggregateRootObject;
                return s.Name;
            }
        }

        /// <summary>
        /// Gets the window text.
        /// </summary>
        /// <value>The window text.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-06-02
        /// </remarks>
        public override string WindowText
        {
            get { return UserTexts.Resources.NewSkillThreeDots; }
        }

        /// <summary>
        /// Creates the new root.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public override ISkill CreateNewRoot()
        {
            if (_preselectedSkillType == null)
            {
				_preselectedSkillType =
					new SkillTypePhone(new Description("Skill type"), ForecastSource.InboundTelephony);
			}
            var culture = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
            var newSkill = new Skill(UserTexts.Resources.LessThanSkillNameGreaterThan,
                                       string.Format(culture, UserTexts.Resources.SkillCreatedDotParameter0,
                                                     DateTime.Now), Color.FromArgb(0),
                                       _preselectedSkillType.DefaultResolution, _preselectedSkillType);
            SetSkillDefaultSettings(newSkill);

            return newSkill;
        }

        /// <summary>
        /// Sets the skill default settings.
        /// </summary>
        /// <param name="newSkill">The new skill.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-23
        /// </remarks>
        public void SetSkillDefaultSettings(ISkill newSkill)
        {
            newSkill.TimeZone = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone;
            var culture = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
            ServiceAgreement serviceAgreement = ServiceAgreement.DefaultValues();
            DateTime startDateUtc = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, newSkill.TimeZone);
            DateTimePeriod timePeriod = new DateTimePeriod(
                startDateUtc, startDateUtc.AddDays(1)).MovePeriod(newSkill.MidnightBreakOffset);
            TemplateSkillDataPeriod templateSkillDataPeriod = new TemplateSkillDataPeriod(serviceAgreement, new SkillPersonData(), timePeriod);
            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                SkillDayTemplate skillDayTemplate = new SkillDayTemplate(
                    string.Format(culture, "<{0}>",
                                  culture.DateTimeFormat.GetAbbreviatedDayName(
                                      dayOfWeek).ToUpper(culture)),
                    new List<ITemplateSkillDataPeriod> {(TemplateSkillDataPeriod) templateSkillDataPeriod.Clone()});
                newSkill.SetTemplateAt((int)dayOfWeek, skillDayTemplate);
            }
        }
    }
}