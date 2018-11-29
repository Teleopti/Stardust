using System.Windows.Forms;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands
{
    public interface IOpenMeetingsOverviewCommand :IExecutableCommand , ICanExecute{}

    public class OpenMeetingsOverviewCommand :IOpenMeetingsOverviewCommand
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPersonSelectorPresenter _personSelectorPresenter;
        private readonly IMeetingOverviewViewFactory _meetingOverviewViewFactory;

        private readonly bool _canExecute =
            PrincipalAuthorization.Current().IsPermitted(
                DefinedRaptorApplicationFunctionPaths.ViewSchedules);

        public OpenMeetingsOverviewCommand( IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory,
                IPersonSelectorPresenter personSelectorPresenter, IMeetingOverviewViewFactory meetingOverviewViewFactory)
        {
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
            _personSelectorPresenter = personSelectorPresenter;
            _meetingOverviewViewFactory = meetingOverviewViewFactory;
        }

        public void Execute()
        {
            var view = _personSelectorPresenter.View;
            view.Cursor = Cursors.WaitCursor;
            
            try
            {
                IScenario defaultScenario;
                using (IUnitOfWork unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    defaultScenario = _repositoryFactory.CreateScenarioRepository(unitOfWork).LoadDefaultScenario();
                }
                var period = new DateOnlyPeriod(_personSelectorPresenter.SelectedDate,
                                                _personSelectorPresenter.SelectedDate);
                _meetingOverviewViewFactory.Create(_personSelectorPresenter.SelectedPersonGuids, period, defaultScenario);
            }
            catch (DataSourceException dataSourceException)
            {
                view.ShowDataSourceException(dataSourceException, Resources.MeetingOverview);
            }
            view.Cursor = Cursors.Default;
        }

        public bool CanExecute()
        {
            return _canExecute;
        }
    }
}