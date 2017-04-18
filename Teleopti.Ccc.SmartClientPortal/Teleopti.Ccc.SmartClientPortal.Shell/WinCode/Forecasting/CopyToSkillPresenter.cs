using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting
{
    public class CopyToSkillPresenter
    {
        private readonly ICopyToSkillView _view;
        private readonly CopyToSkillModel _model;
        private readonly ICopyToSkillCommand _copyToSkillCommand;
        private readonly ISkillRepository _skillRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISpecification<ISkill> _possibleTargetSkillSpecification;

        public CopyToSkillPresenter(ICopyToSkillView view, CopyToSkillModel model, ICopyToSkillCommand copyToSkillCommand, ISkillRepository skillRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _view = view;
            _model = model;
            _copyToSkillCommand = copyToSkillCommand;
            _skillRepository = skillRepository;
            _unitOfWorkFactory = unitOfWorkFactory;

            _possibleTargetSkillSpecification = new IsPossibleTargetSkillForCopy(model.Workload.Skill);
        }

        public void Initialize()
        {
            _view.SetCopyFromText(_model.SourceWorkloadName);
            _view.ToggleIncludeTemplates(_model.IncludeTemplates);
            _view.ToggleIncludeQueues(_model.IncludeQueues);

            SetAvailableSkills();
        }

        private void SetAvailableSkills()
        {
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var skills = _skillRepository.FindAllWithWorkloadAndQueues();
                var possibleSkills = skills.FilterBySpecification(_possibleTargetSkillSpecification);
                if (!possibleSkills.Any())
                {
                    _view.NoMatchingSkillsAvailable();
                    _view.Close();
                }
                foreach (ISkill skill in possibleSkills)
                {
                    _view.AddSkillToList(skill.Name, skill);
                }
            }
        }

        public void Copy()
        {
            _copyToSkillCommand.Execute();
        }

        public void ToggleIncludeTemplates(bool included)
        {
            _model.IncludeTemplates = included;
        }

        public void ToggleIncludeQueues(bool included)
        {
            _model.IncludeQueues = included;
        }

        public void SetTargetSkill(ISkill skill)
        {
            _model.TargetSkill = skill;
        }
    }
}