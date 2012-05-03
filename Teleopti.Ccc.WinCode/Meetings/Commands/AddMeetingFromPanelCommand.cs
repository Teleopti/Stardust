using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Meetings.Commands
{
    public interface IAddMeetingFromPanelCommand : IExecutableCommand, ICanExecute
    {
    }

    public class AddMeetingFromPanelCommand : IAddMeetingFromPanelCommand
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPersonSelectorPresenter _personSelectorPresenter;
        private readonly IMeetingOverviewViewFactory _meetingOverviewViewFactory;

        private readonly bool _canExecute =
            PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyMeetings);

        public AddMeetingFromPanelCommand(IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory,
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
                bool viewSchedulesPermission;
                IMeetingViewModel meetingViewModel;
                using (IUnitOfWork unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    var activities = _repositoryFactory.CreateActivityRepository(unitOfWork).LoadAllSortByName();
                    if (activities == null || activities.Count == 0)
                    {
                        view.Cursor = Cursors.Default;
                        return;
                    }

                    IScenario defaultScenario = _repositoryFactory.CreateScenarioRepository(unitOfWork).LoadDefaultScenario();
                    var rep = _repositoryFactory.CreatePersonRepository(unitOfWork);
                    IPerson organizer = TeleoptiPrincipal.Current.GetPerson(rep);

                    ISettingDataRepository settingDataRepository =
                            _repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork);

                    var commonNameDescription = settingDataRepository.FindValueByKey("CommonNameDescription",
                                                                                            new CommonNameDescriptionSetting());

                    var pers = rep.FindPeople(_personSelectorPresenter.SelectedPersonGuids);

                    viewSchedulesPermission = isPermittedToViewSchedules();
                    meetingViewModel = MeetingComposerPresenter.CreateDefaultMeeting(organizer,
                                        defaultScenario, activities[0], DateOnly.Today.AddDays(1),
                                        pers, commonNameDescription, organizer.PermissionInformation.DefaultTimeZone());

                }

                _meetingOverviewViewFactory.ShowMeetingComposerView(view, meetingViewModel, viewSchedulesPermission);
            }
            catch (DataSourceException dataSourceException)
            {
                view.ShowDataSourceException(dataSourceException, Resources.AddMeeting);
            }
            
            view.Cursor = Cursors.Default;
        }

        public bool CanExecute()
        {
            return _canExecute;
        }

        private static bool isPermittedToViewSchedules()
        {
            IPerson person = ((IUnsafePerson)TeleoptiPrincipal.Current).Person;
            ITeam rightClickedPersonsTeam = person.MyTeam(DateOnly.Today);
            if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules, DateOnly.Today, rightClickedPersonsTeam))
            {
                return true;
            }
            return false;

        }
    }
}