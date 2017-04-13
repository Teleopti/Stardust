using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Commands
{
    public interface IPersonFinderPreviousCommand : IExecutableCommand, ICanExecute
    {
        new void Execute();
        new bool CanExecute();
    }

    public class PersonFinderPreviousCommand : IPersonFinderPreviousCommand
    {
         private readonly IPersonFinderModel _model;

         public PersonFinderPreviousCommand(IPersonFinderModel model)
        {
            _model = model;
        }

        public void Execute()
        {
            _model.SearchCriteria.CurrentPage--;
            _model.Find();
        }

        public bool CanExecute()
        {
            return _model.SearchCriteria.CurrentPage > 1;
        }
    }
}
