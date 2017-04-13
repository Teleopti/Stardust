using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting
{
    public class CopyToSkillCommand : ICopyToSkillCommand
    {
        private readonly ICopyToSkillView _view;
        private readonly CopyToSkillModel _model;
        private readonly IWorkloadRepository _workloadRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public CopyToSkillCommand(ICopyToSkillView view, CopyToSkillModel model, IWorkloadRepository workloadRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _view = view;
            _model = model;
            _workloadRepository = workloadRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public void Execute()
        {
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                unitOfWork.Reassociate(_model.TargetSkill);
                unitOfWork.Reassociate(_model.Workload);

                IWorkload workload = new Workload(_model.TargetSkill);
                workload.QueueAdjustments = _model.Workload.QueueAdjustments;
                workload.Name = string.Concat(UserTexts.Resources.CopyOf, " ", _model.Workload.Name);
                workload.Description = _model.Workload.Description;
                
                if (_model.IncludeQueues)
                {
                    AddQueues(workload);
                }

                if (_model.IncludeTemplates)
                {
                    AddTemplates(workload);
                }

                _workloadRepository.Add(workload);

                var changes = unitOfWork.PersistAll();
                _view.TriggerEntitiesNeedRefresh(changes);
            }
        }

        private void AddTemplates(IWorkload workload)
        {
            foreach (KeyValuePair<int, IWorkloadDayTemplate> workloadDayTemplate in _model.Workload.TemplateWeekCollection)
            {
                var clone = workloadDayTemplate.Value.NoneEntityClone();
                
                WorkloadDayTemplate newTemplate = new WorkloadDayTemplate();
                newTemplate.Create(clone.Name,DateTime.UtcNow,workload,clone.OpenHourList.ToList());
                newTemplate.SetTaskPeriodCollection(clone.TaskPeriodList);

                var skillResolution = TimeSpan.FromMinutes(_model.Workload.Skill.DefaultResolution);
                var mergedPeriods = clone.TaskPeriodList.Where(t => t.Period.ElapsedTime() > skillResolution);

                foreach (var mergedPeriod in mergedPeriods)
                {
                    var periodsToMerge = newTemplate.TaskPeriodList.Where(t => mergedPeriod.Period.Contains(t.Period));
                    newTemplate.MergeTemplateTaskPeriods(periodsToMerge.ToList());
                }

                workload.SetTemplateAt(workloadDayTemplate.Key, newTemplate);
            }
        }

        private void AddQueues(IWorkload workload)
        {
            foreach (var queueSource in _model.Workload.QueueSourceCollection)
            {
                workload.AddQueueSource(queueSource);
            }
        }
    }
}