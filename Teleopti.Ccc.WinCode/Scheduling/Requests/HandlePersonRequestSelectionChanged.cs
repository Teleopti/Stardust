using System;
using System.Collections.Generic;
using System.Linq;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IList<PersonRequestViewModel> GetSelectedModels()
        {
            return _selectedModels.Where(m => m.IsSelected).ToList(); 
           
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
