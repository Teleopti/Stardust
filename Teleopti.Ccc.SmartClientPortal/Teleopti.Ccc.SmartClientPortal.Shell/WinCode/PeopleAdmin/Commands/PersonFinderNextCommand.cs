using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands
{
    public interface IPersonFinderNextCommand : IExecutableCommand, ICanExecute
    {
        new void Execute();
        new bool CanExecute();
    }

    public class PersonFinderNextCommand : IPersonFinderNextCommand
    {
        private readonly IPersonFinderModel _model;

        public PersonFinderNextCommand(IPersonFinderModel model)
        {
            _model = model;
        }
        public void Execute()
        {
            _model.SearchCriteria.CurrentPage++;
            _model.Find();
        }

        public bool CanExecute()
        {
        	return _model.SearchCriteria.CurrentPage < _model.SearchCriteria.TotalPages;
        }
    }
}
