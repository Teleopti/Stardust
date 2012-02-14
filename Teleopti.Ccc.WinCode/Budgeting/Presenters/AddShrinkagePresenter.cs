using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Ccc.WinCode.Budgeting.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
    public class AddShrinkagePresenter
    {
        private readonly IAddShrinkageForm _view;
        private readonly AddShrinkageModel _model;

        public AddShrinkagePresenter(IAddShrinkageForm view, AddShrinkageModel model)
        {
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            Absences = _model.LoadAbsences();
        }
        
        public IEnumerable<IAbsence> Absences { get; private set; }

        public ICustomShrinkage CustomShrinkageAdded
        {
            get { return _model.CustomShrinkageAdded; }
        }

        public void Save(ICustomShrinkage customShrinkage)
        {
            _model.Save(customShrinkage);
        }

        public void RemoveAllAbsences()
        {
            _view.RemoveSelectedAbsences();
        }

        public void AddAbsences()
        {
            _view.AddSelectedAbsences();
        }
    }
}