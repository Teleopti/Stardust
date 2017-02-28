using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public interface IOpenIntradayTodayCommand :ICanExecute
    {
        
    }
    public class OpenIntradayTodayCommand : IOpenIntradayTodayCommand

{
        private readonly IPersonSelectorView _personSelectorView;

        public OpenIntradayTodayCommand(IPersonSelectorView personSelectorView)
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