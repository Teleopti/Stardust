using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public static class ForecastingTemplateRefresher
    {
        public static void RefreshRoot(IForecastTemplateOwner root)
        {
            IForecastTemplateOwner newTemplateOwnerInstance;
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                newTemplateOwnerInstance = LoadNewInstance(root, uow);
                if (newTemplateOwnerInstance == null) return;

                InitializeSkillGraph(newTemplateOwnerInstance);
            }

            root.RefreshTemplates(newTemplateOwnerInstance);
        }

        public static IForecastTemplateOwner LoadNewInstance(IForecastTemplateOwner owner, IUnitOfWork uow)
        {
            IRepositoryFactory repositoryFactory = new RepositoryFactory();

            IWorkload workload = owner as IWorkload;
            if (workload != null)
            {
                IWorkloadRepository workloadRepository = repositoryFactory.CreateWorkloadRepository(uow);
                return workloadRepository.Get(workload.Id.Value);
            }

            IMultisiteSkill multisiteSkill = owner as IMultisiteSkill;
            if (multisiteSkill != null)
            {
                MultisiteSkillRepository multisiteSkillRepository = MultisiteSkillRepository.DONT_USE_CTOR(uow);
                return multisiteSkillRepository.Get(multisiteSkill.Id.Value);
            }

            ISkill skill = owner as ISkill;
            if (skill != null)
            {
                ISkillRepository skillRepository = repositoryFactory.CreateSkillRepository(uow);
                return skillRepository.Get(skill.Id.Value);
            }
            return null;
        }

        public static void InitializeSkillGraph(IForecastTemplateOwner root)
        {
            if (!LazyLoadingManager.IsInitialized(root))
                LazyLoadingManager.Initialize(root);
            
            InitializeSkill(root);

            InitializeWorkload(root);

            InitializeMultisiteSkill(root);

            InitializeChildSkill(root);
        }

        private static void InitializeChildSkill(IForecastTemplateOwner root)
        {
            IChildSkill childSkill = root as IChildSkill;
            if (childSkill != null)
            {
                if (!LazyLoadingManager.IsInitialized(childSkill.WorkloadCollection)) 
                    LazyLoadingManager.Initialize(childSkill.WorkloadCollection);
                foreach (KeyValuePair<int, ISkillDayTemplate> keyValuePair in childSkill.TemplateWeekCollection)
                {
                    if (!LazyLoadingManager.IsInitialized(keyValuePair.Value.TemplateSkillDataPeriodCollection)) 
                        LazyLoadingManager.Initialize(keyValuePair.Value.TemplateSkillDataPeriodCollection);
                }
            }
        }

        private static void InitializeMultisiteSkill(IForecastTemplateOwner root)
        {
            IMultisiteSkill multisiteSkill = root as IMultisiteSkill;
            if (multisiteSkill != null)
            {
                if (!LazyLoadingManager.IsInitialized(multisiteSkill.WorkloadCollection)) 
                    LazyLoadingManager.Initialize(multisiteSkill.WorkloadCollection);
                if (!LazyLoadingManager.IsInitialized(multisiteSkill.ChildSkills)) 
                    LazyLoadingManager.Initialize(multisiteSkill.ChildSkills);
                foreach (KeyValuePair<int, IMultisiteDayTemplate> keyValuePair in multisiteSkill.TemplateMultisiteWeekCollection)
                {
                    if (!LazyLoadingManager.IsInitialized(keyValuePair.Value.TemplateMultisitePeriodCollection)) 
                        LazyLoadingManager.Initialize(keyValuePair.Value.TemplateMultisitePeriodCollection);
                }
            }
        }

        private static void InitializeWorkload(IForecastTemplateOwner root)
        {
            IWorkload workload = root as IWorkload;
            if (workload != null)
            {
                if (!LazyLoadingManager.IsInitialized(workload.Skill)) 
                    LazyLoadingManager.Initialize(workload.Skill);
                if (!LazyLoadingManager.IsInitialized(workload.Skill.SkillType)) 
                    LazyLoadingManager.Initialize(workload.Skill.SkillType);
                if (!LazyLoadingManager.IsInitialized(workload.TemplateWeekCollection)) 
                    LazyLoadingManager.Initialize(workload.TemplateWeekCollection);
                if (!LazyLoadingManager.IsInitialized(workload.QueueSourceCollection)) 
                    LazyLoadingManager.Initialize(workload.QueueSourceCollection);
                foreach (KeyValuePair<int, IWorkloadDayTemplate> keyValuePair in workload.TemplateWeekCollection)
                {
                    if (!LazyLoadingManager.IsInitialized(keyValuePair.Value.OpenHourList)) 
                        LazyLoadingManager.Initialize(keyValuePair.Value.OpenHourList);
                    if (!LazyLoadingManager.IsInitialized(keyValuePair.Value.TaskPeriodList)) 
                        LazyLoadingManager.Initialize(keyValuePair.Value.TaskPeriodList);
                }
            }
        }

        private static void InitializeSkill(IForecastTemplateOwner root)
        {
            ISkill skill = root as ISkill;
            if (skill != null)
            {
                if (!LazyLoadingManager.IsInitialized(skill.SkillType))
                    LazyLoadingManager.Initialize(skill.SkillType);
                if (!LazyLoadingManager.IsInitialized(skill.WorkloadCollection)) 
                    LazyLoadingManager.Initialize(skill.WorkloadCollection);
                foreach (KeyValuePair<int, ISkillDayTemplate> keyValuePair in skill.TemplateWeekCollection)
                {
                    if (!LazyLoadingManager.IsInitialized(keyValuePair.Value.TemplateSkillDataPeriodCollection))
                        LazyLoadingManager.Initialize(keyValuePair.Value.TemplateSkillDataPeriodCollection);
                }
            }
        }
    }
}
