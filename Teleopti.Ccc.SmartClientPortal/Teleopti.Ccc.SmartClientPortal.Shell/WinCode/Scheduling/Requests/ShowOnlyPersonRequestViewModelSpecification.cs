using System.Collections.Generic;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
{
    public class ShowOnlyPersonRequestViewModelSpecification : Specification<PersonRequestViewModel>
    {

        private IList<PersonRequestViewModel> _modelsToShow;

        public ShowOnlyPersonRequestViewModelSpecification(IList<PersonRequestViewModel> modelsToShow)
        {
            _modelsToShow = modelsToShow;
        }
        public override bool IsSatisfiedBy(PersonRequestViewModel obj)
        {
            return !_modelsToShow.Contains(obj);
        }

    }
}
