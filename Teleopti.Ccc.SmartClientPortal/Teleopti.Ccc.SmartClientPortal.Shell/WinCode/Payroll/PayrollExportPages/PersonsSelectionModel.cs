using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages
{
    public class PersonsSelectionModel
    {
        private readonly HashSet<IPerson> _selectedPersons = new HashSet<IPerson>();

        public void AddPerson(IPerson person)
        {
            _selectedPersons.Add(person);
        }

        public IEnumerable<IPerson> SelectedPersons => _selectedPersons;

	    public DateOnlyPeriod SelectedPeriod
        {
            get; set; 
        }

        public IApplicationFunction ApplicationFunction()
        {
            return Domain.Security.AuthorizationEntities.ApplicationFunction.FindByPath(
                new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
                DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication);
        }
    }
}
