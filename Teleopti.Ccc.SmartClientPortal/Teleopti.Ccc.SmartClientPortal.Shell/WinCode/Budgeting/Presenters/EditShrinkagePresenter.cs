using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
    public class EditShrinkagePresenter
    {
        private readonly IEditShrinkageForm _view;
        private readonly EditShrinkageModel _model;

        public EditShrinkagePresenter(IEditShrinkageForm view, EditShrinkageModel model)
        {
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            Absences = _model.LoadAbsences();
            AddedAbsences = _model.AddedAbsences();
        }
        
        public IEnumerable<IAbsence> Absences { get; private set; }

        public IEnumerable<IAbsence> AddedAbsences { get; private set; }

        public void Save(ICustomShrinkage customShrinkage)
        {
            _model.Save(customShrinkage);
        }

        public string ShrinkageName
        {
            get { return _model.ShrinakgeName; }
        }

        public bool IncludedInAllowance
        {
            get { return _model.IncludedInAllowance; }
        }

        public Guid ShrinkageId
        {
            get { return _model.ShrinkageId; }
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