using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AuditHistory
{
    public interface IAuditHistoryModel
    {

        void First();
        void Later();
        void Earlier();

        int CurrentPage { get; }
        int NumberOfPages { get; }
        string HeaderText { get; }

        IList<RevisionDisplayRow> PageRows { get; }
        IScheduleDay SelectedScheduleDay { get; set; }
        IScheduleDay CurrentScheduleDay { get; }
    }

    public class AuditHistoryModel : IAuditHistoryModel
    {
        private readonly IScheduleDay _currentScheduleDay;
        private readonly IScheduleHistoryRepository _scheduleHistoryRepository;
        private readonly IAuditHistoryScheduleDayCreator _auditHistoryScheduleDayCreator;
        private IList<IRevision> _revisions = new List<IRevision>();
        private IList<RevisionDisplayRow> _pageRows = new List<RevisionDisplayRow>();
        private const int pageSize = 10;
        private int _currentPage = 1;
        
        public IScheduleDay SelectedScheduleDay { get; set; }

        public AuditHistoryModel(IScheduleDay currentScheduleDay, IScheduleHistoryRepository scheduleHistoryRepository, IAuditHistoryScheduleDayCreator auditHistoryScheduleDayCreator)
        {
            _currentScheduleDay = currentScheduleDay;
            _scheduleHistoryRepository = scheduleHistoryRepository;
            _auditHistoryScheduleDayCreator = auditHistoryScheduleDayCreator;
        }

        public void First()
        {
            getRevisions();
            _currentPage = 1;
            IEnumerable<IRevision> pageRevisions = revisionsForPage(_currentPage);
            _pageRows = getPageRows(pageRevisions);
        }

        public void Later()
        {
            _currentPage -= 1;
            IEnumerable<IRevision> pageRevisions = revisionsForPage(_currentPage);
            _pageRows = getPageRows(pageRevisions);
        }

        public void Earlier()
        {
            _currentPage += 1;
            IEnumerable<IRevision> pageRevisions = revisionsForPage(_currentPage);
            _pageRows = getPageRows(pageRevisions);
        }

        public int CurrentPage => _currentPage;

        public int NumberOfPages
        {
            get
            {
                double tmp = (double)_revisions.Count/pageSize;
                return (int)Math.Ceiling(tmp);
            }
        }

        public string HeaderText => _currentScheduleDay.DateOnlyAsPeriod.DateOnly.Date.ToShortDateString() + " - " + _currentScheduleDay.Person.Name;

        public IList<RevisionDisplayRow> PageRows => _pageRows;

        private IEnumerable<IRevision> revisionsForPage(int pageNumber)
        {
            IList<IRevision> retList = new List<IRevision>();
            int startIndex = (pageNumber - 1)*pageSize;
            for (int i = startIndex; i < startIndex + pageSize; i++)
            {
                if(i<_revisions.Count)
                    retList.Add(_revisions[i]);
            }

            return retList;
        }

        private IList<RevisionDisplayRow> getPageRows(IEnumerable<IRevision> revisions)
        {
            IList<RevisionDisplayRow> retList = new List<RevisionDisplayRow>();

            foreach (var revision in revisions)
            {
                var result = _scheduleHistoryRepository.FindSchedules(revision,_currentScheduleDay.Person,_currentScheduleDay.DateOnlyAsPeriod.DateOnly);

                IList<IPersonAssignment> personAssignments = new List<IPersonAssignment>();
                IList<IPersonAbsence> personAbsences = new List<IPersonAbsence>();
                
                foreach (var data in result)
                {
                    var personAssignment = data as IPersonAssignment;
                    var personAbsence = data as IPersonAbsence;

                    if(personAssignment != null) personAssignments.Add(personAssignment);
                    if(personAbsence != null) personAbsences.Add(personAbsence);
                }

                if (!personAssignments.IsEmpty())
                {
                    var initializerPersonAssignment = new InitializeRootsPersonAssignment(personAssignments);
                    initializerPersonAssignment.Initialize();
                }

                if(!personAbsences.IsEmpty())
                {
                    var initializerPersonAbsence = new InitializeRootsPersonAbsence(personAbsences);
                    initializerPersonAbsence.Initialize();
                }

	            var historicalDay = _currentScheduleDay.ReFetch();
                _auditHistoryScheduleDayCreator.Apply(historicalDay, result);
                var changedOnLocal = TimeZoneHelper.ConvertFromUtc(revision.ModifiedAt, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
                var row = new RevisionDisplayRow{ScheduleDay = historicalDay, Name = revision.ModifiedBy.Name.ToString(), ChangedOn = changedOnLocal};
                retList.Add(row);
            }

            return retList;
        }

        private void getRevisions()
        {
            IEnumerable<IRevision> revisioner = _scheduleHistoryRepository.FindRevisions(_currentScheduleDay.Person, _currentScheduleDay.DateOnlyAsPeriod.DateOnly, 10000);
            _revisions = new List<IRevision>(revisioner);
        }


        public IScheduleDay CurrentScheduleDay => _currentScheduleDay;
    }
}