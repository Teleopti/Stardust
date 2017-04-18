using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands
{
	public interface IPersonFinderFindCommand : IExecutableCommand, ICanExecute
	{
		new void Execute();
		new bool CanExecute();
	}

	public class PersonFinderFindCommand : IPersonFinderFindCommand
	{
		private readonly IPersonFinderModel _model;

		public PersonFinderFindCommand(IPersonFinderModel model)
		{
			_model = model;
		}

		public void Execute()
		{
			_model.Find();
		}

		public bool CanExecute()
		{
			return _model != null && _model.SearchCriteria.SearchValue.Length != 0;
		}
	}
}
