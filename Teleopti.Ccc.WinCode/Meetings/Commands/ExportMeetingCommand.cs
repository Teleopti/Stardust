using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Meetings.Events;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.WinCode.Meetings.Overview;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Meetings.Commands
{
    public interface IExportMeetingCommand : IExecutableCommand, ICanExecute
    {
    }

    public class ExportMeetingCommand : IExportMeetingCommand
    {
        private readonly IExportMeetingView _exportMeetingView;
        private readonly IMeetingRepository _meetingRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IExportMeetingsProvider _exportMeetingsProvider;
        private readonly IEventAggregator _eventAggregator;
        
        public ExportMeetingCommand(IExportMeetingView exportMeetingView, IMeetingRepository meetingRepository,
            IUnitOfWorkFactory unitOfWorkFactory, IExportMeetingsProvider exportMeetingsProvider, IEventAggregator eventAggregator)
        {
            _exportMeetingView = exportMeetingView;
            _meetingRepository = meetingRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _exportMeetingsProvider = exportMeetingsProvider;
            _eventAggregator = eventAggregator;
        }

        public void Execute()
        {
            if(!CanExecute())
                return;

            var meetingsToExport = _exportMeetingsProvider.GetMeetings(_exportMeetingView.SelectedDates[0]);
            using (var uow =_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var toScenario = _exportMeetingView.SelectedScenario;
                var earlierExports = _meetingRepository.FindMeetingsWithTheseOriginals(meetingsToExport, toScenario);
                foreach (var earlierExport in earlierExports)
                {
					earlierExport.Snapshot();
                    _meetingRepository.Remove(earlierExport);
                }
                foreach (var meeting in meetingsToExport)
                {
                    var clone = meeting.NoneEntityClone();
                    clone.SetScenario(toScenario);
                    // Don't do this in clone, it's only i  export to other scenario, or????
                    clone.OriginalMeetingId = meeting.OriginalMeetingId;
                    _meetingRepository.Add(clone);
                }
                uow.PersistAll();
                _eventAggregator.GetEvent<MeetingExportFinished>().Publish(meetingsToExport.Count);
            }
        }

        public bool CanExecute()
        {
            if (_exportMeetingView.SelectedScenario == null)
                return false;
            return _exportMeetingView.SelectedDates.Count > 0;
        }
    }
}