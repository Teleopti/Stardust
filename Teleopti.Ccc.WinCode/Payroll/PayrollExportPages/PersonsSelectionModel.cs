﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll.PayrollExportPages
{
    public class PersonsSelectionModel
    {
        private readonly HashSet<IPerson> _selectedPersons = new HashSet<IPerson>();

        public void AddPerson(IPerson person)
        {
            _selectedPersons.Add(person);
        }

        public void RemovePerson(IPerson person)
        {
            _selectedPersons.Remove(person);
        }

        public IEnumerable<IPerson> SelectedPersons
        {
            get { return _selectedPersons; }
        }

        public DateOnlyPeriod SelectedPeriod
        {
            get; set; 
        }

        public IApplicationFunction ApplicationFunction()
        {
            return Domain.Security.AuthorizationEntities.ApplicationFunction.FindByPath(
                new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
                DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication);
        }
    }
}
