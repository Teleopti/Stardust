using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IOpenModuleCommand : ICanExecute //, IExecutableCommand
    { }

    public class OpenModuleCommand : IOpenModuleCommand
    {

        private readonly IPersonSelectorView _personSelectorView;

        public OpenModuleCommand(IPersonSelectorView personSelectorView)
        {
            _personSelectorView = personSelectorView;
        }

        //when we move the code ....
        //public void Execute()
        //{
        //    throw new NotImplementedException();
        //}

        public bool CanExecute()
        {
            return _personSelectorView.SelectedNodes.Count > 0;
        }
    }
}