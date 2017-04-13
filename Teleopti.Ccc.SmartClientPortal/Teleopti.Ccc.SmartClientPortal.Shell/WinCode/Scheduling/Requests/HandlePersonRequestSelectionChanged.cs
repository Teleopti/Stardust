using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    public class HandlePersonRequestSelectionChanged
    {
        private IList<PersonRequestViewModel> _selectedModels;
        public ISpecification<IPersonRequestViewModel> SelectionIsEditableSpecification { get; set; }

        public bool SelectionIsEditable
        {
            get { return _selectedModels.Count(m => SelectionIsEditableSpecification.IsSatisfiedBy(m)) > 0; }
        }

        public HandlePersonRequestSelectionChanged(IList<PersonRequestViewModel> models, IPersonRequestCheckAuthorization authorization):this(models,new PersonRequestViewModelIsEditableSpecification(authorization))
        {
            
        }

        public HandlePersonRequestSelectionChanged(IList<PersonRequestViewModel> models, ISpecification<IPersonRequestViewModel> selectionIsEditableSpecification)
        {
            SelectionIsEditableSpecification = selectionIsEditableSpecification;
            _selectedModels = models;

        }
    }
}
