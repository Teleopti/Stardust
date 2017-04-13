using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Presentation
{
    public class ReportUserSelectorAuditingPresenter
    {
        private readonly IScheduleHistoryReport _scheduleHistoryRepository;
        private readonly IReportUserSelectorAuditingView _view;
        private readonly IUnitOfWorkFactory _unitOfWorkfactory;
        private readonly IList<ReportUserSelectorAuditingModel> _revisionList;

        public ReportUserSelectorAuditingPresenter(IReportUserSelectorAuditingView view, IScheduleHistoryReport scheduleHistoryReport, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _scheduleHistoryRepository = scheduleHistoryReport;
            _view = view;
            _unitOfWorkfactory = unitOfWorkFactory;
            _revisionList = new List<ReportUserSelectorAuditingModel>();
        }

        public IList<ReportUserSelectorAuditingModel> RevisionList
        {
            get{return _revisionList;}
        }

        public void Initialize()
        {
            _revisionList.Add(new ReportUserSelectorAuditingModel(Guid.Empty, UserTexts.Resources.All));

            using (_unitOfWorkfactory.CreateAndOpenUnitOfWork())
            {
	            var sortedPersonByFirstNameLastName =
		            _scheduleHistoryRepository.RevisionPeople()
											.OrderBy(p => p.Name.FirstName)
											.ThenBy(p => p.Name.LastName)
											.ToList();
				foreach (var person in sortedPersonByFirstNameLastName)
                {
                    _revisionList.Add(new ReportUserSelectorAuditingModel(person));
                }
            }

            _view.UpdateUsersCombo(new ReadOnlyCollection<ReportUserSelectorAuditingModel>(_revisionList));
            _view.SetSelectedUser(_revisionList[0]);
        }

        public IList<IPerson> SelectedUsers()
        {
            IList<IPerson> users = new List<IPerson>();
            var selectedUser = _view.SelectedUserModel.Person;

            if (selectedUser == null)
            {
                foreach (var revisionModel in _revisionList)
                {
                    if (revisionModel.Person != null)
                        users.Add(revisionModel.Person);
                }
            }
            else
            {
                users.Add(selectedUser);
            }

            return users;    
        }
    }
}
