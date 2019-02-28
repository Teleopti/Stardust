using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class SaveForecastToScenarioCommand
    {
        private readonly ISkill _skill;
        private readonly IMultisiteSkill _multisiteSkill;
        private ISkillDayCalculator _skillDayCalculator;
        private readonly DateOnlyPeriod _period;
        private IScenario _newScenario;

        public event EventHandler<CustomEventArgs<int>> ProgressReporter;

        public SaveForecastToScenarioCommand(ISkill skill, ISkillDayCalculator skillDayCalculator, DateOnlyPeriod period)
        {
            _skill = skill;
            _multisiteSkill = _skill as IMultisiteSkill;
            _skillDayCalculator = skillDayCalculator;
            _period = period;
        }

        private bool IsMultisiteSkill { get { return _multisiteSkill != null; } }
        
        public SaveForecastToScenarioCommandResult Execute(IScenario newScenario)
        {
            _newScenario = newScenario;
            var unsavedSkillDays = IsMultisiteSkill ? saveAsScenarioMultisite() : saveAsScenarioSingleSite();
            var result = new SaveForecastToScenarioCommandResult(_newScenario, _skillDayCalculator, unsavedSkillDays);
            return result;
        }

        private IUnsavedDaysInfo saveAsScenarioSingleSite()
        {
            var unsavedSkillDays = new UnsavedDaysInfo();
            deleteExistingSkillDays();
            
            _skillDayCalculator = _skillDayCalculator.CloneToScenario(_newScenario);
            saveSkillDays(unsavedSkillDays);

            return unsavedSkillDays;
        }

        private IUnsavedDaysInfo saveAsScenarioMultisite()
        {
            var unsavedSkillDays = new UnsavedDaysInfo();
            var skillDayCalculator = _skillDayCalculator as MultisiteSkillDayCalculator;
            if (skillDayCalculator == null) return unsavedSkillDays;

            deleteExistingMultisiteSkillDays();
            _skillDayCalculator = _skillDayCalculator.CloneToScenario(_newScenario);

            //Add new entities to correct repository
            skillDayCalculator = _skillDayCalculator as MultisiteSkillDayCalculator;
            if (skillDayCalculator == null) return unsavedSkillDays;
            
            saveSkillDays(unsavedSkillDays);
            saveMultisiteDays(skillDayCalculator, unsavedSkillDays);
            saveChildSkillDays(skillDayCalculator, unsavedSkillDays);

            return unsavedSkillDays;
        }
        
        private void deleteExistingSkillDays()
        {
            using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ISkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(unitOfWork);
                // Delete existing Skilldays within same period, Skill and Scenario.
                skillDayRepository.Delete(_period, _skill, _newScenario);
                unitOfWork.PersistAll();
                
            }
        }
        
        private void savePerSkillDay(ISkillDay skillDay, IUnsavedDaysInfo unsavedSkillDays)
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                try
                {
                    var skillDayRepository = SkillDayRepository.DONT_USE_CTOR(uow);
                    skillDayRepository.Add(skillDay);
                    uow.PersistAll();
                }
                catch (OptimisticLockException)
                {
                    addToUnsavedDays(skillDay.CurrentDate, unsavedSkillDays);
                }
                catch (ConstraintViolationException)
                {
                    addToUnsavedDays(skillDay.CurrentDate, unsavedSkillDays);
                }
                InvokeProgressReporter(new CustomEventArgs<int>(1));
            }
        }

      

        private void savePerMultisiteDay(IMultisiteDay multisiteDay, IUnsavedDaysInfo unsavedSkillDays)
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                try
                {
                    var multisiteDayRepository = MultisiteDayRepository.DONT_USE_CTOR(uow);
                    multisiteDayRepository.Add(multisiteDay);
                    uow.PersistAll();
                }
                catch (OptimisticLockException)
                {
                    addToUnsavedDays(multisiteDay.MultisiteDayDate, unsavedSkillDays);
                }
                catch (ConstraintViolationException)
                {
                    addToUnsavedDays(multisiteDay.MultisiteDayDate, unsavedSkillDays);
                }
                InvokeProgressReporter(new CustomEventArgs<int>(1));
            }
        }

        private void addToUnsavedDays(DateOnly date, IUnsavedDaysInfo unsavedSkillDays)
        {
            var unsavedDay = new UnsavedDayInfo(date, _newScenario);
            if (!unsavedSkillDays.Contains(unsavedDay))
                unsavedSkillDays.Add(unsavedDay);
        }

        private void deleteExistingMultisiteSkillDays()
        {
            using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ISkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(unitOfWork);
                var multisiteDayRepository = MultisiteDayRepository.DONT_USE_CTOR(unitOfWork);
                // Delete existing Skilldays within same period, Skill and Scenario.
                skillDayRepository.Delete(_period, _skill, _newScenario);
                foreach (var childSkill in _multisiteSkill.ChildSkills)
                {
                    skillDayRepository.Delete(_period, childSkill, _newScenario);
                }
                multisiteDayRepository.Delete(_period, _multisiteSkill, _newScenario);
                unitOfWork.PersistAll();
            }
        }

        private void saveChildSkillDays(MultisiteSkillDayCalculator skillDayCalculator, IUnsavedDaysInfo unsavedSkillDays)
        {
            foreach (var childSkill in _multisiteSkill.ChildSkills)
            {
                foreach (var skillDay in skillDayCalculator.GetVisibleChildSkillDays(childSkill))
                {
                    savePerSkillDay(skillDay, unsavedSkillDays);
                }
            }
        }

        private void saveMultisiteDays(MultisiteSkillDayCalculator skillDayCalculator, IUnsavedDaysInfo unsavedSkillDays)
        {
            foreach (var multisiteDay in skillDayCalculator.VisibleMultisiteDays)
            {
                savePerMultisiteDay(multisiteDay, unsavedSkillDays);
            }
        }

        private void saveSkillDays(IUnsavedDaysInfo unsavedSkillDays)
        {
            foreach (var skillDay in _skillDayCalculator.VisibleSkillDays)
            {
                savePerSkillDay(skillDay, unsavedSkillDays);
            }
        }

        public void InvokeProgressReporter(CustomEventArgs<int> e)
        {
			ProgressReporter?.Invoke(this, e);
		}
    }

    public class SaveForecastToScenarioCommandResult
    {
        private readonly IScenario _newScenario;
        private readonly ISkillDayCalculator _skillDayCalculator;
        private readonly IUnsavedDaysInfo _unsavedDaysInfo;
        
        public IScenario NewScenario { get { return _newScenario; } }
        public ISkillDayCalculator SkillDayCalculator { get { return _skillDayCalculator; } }
        public IUnsavedDaysInfo UnsavedDaysInfo {get {return _unsavedDaysInfo;}}

        public SaveForecastToScenarioCommandResult(IScenario newScenario, ISkillDayCalculator skillDayCalculator, IUnsavedDaysInfo unsavedDaysInfo)
        {
            _newScenario = newScenario;
            _skillDayCalculator = skillDayCalculator;
            _unsavedDaysInfo = unsavedDaysInfo;
        }

    }
}