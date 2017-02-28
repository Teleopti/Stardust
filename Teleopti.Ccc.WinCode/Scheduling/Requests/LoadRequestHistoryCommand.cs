using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ILoadRequestHistoryCommand : IExecutableCommand
    { }

    public class LoadRequestHistoryCommand : ILoadRequestHistoryCommand
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IRequestHistoryView _requestHistoryView;

        public LoadRequestHistoryCommand(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IRequestHistoryView requestHistoryView)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _requestHistoryView = requestHistoryView;
        }

        public void Execute()
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                Guid selectedPerson = _requestHistoryView.SelectedPerson;
                int startRow = _requestHistoryView.StartRow;
                int pageSize = _requestHistoryView.PageSize;
                _requestHistoryView.TotalCount = 0;
                var requests = _repositoryFactory.CreateRequestHistoryReadOnlyRepository(uow).LoadOnPerson(selectedPerson, startRow, startRow + pageSize);
                if (requests.Count > 0)
                    _requestHistoryView.TotalCount = requests[0].TotalCount;
                //skapa listviewitems
                var list = new List<ListViewItem>();
                foreach (var request in requests)
                {
                    var item = new ListViewItem(request.RequestTypeText) { Tag = request };
                    item.SubItems.Add(request.RequestStatusText);
                    item.SubItems.Add(request.ShortDates);
                    item.SubItems.Add(request.Subject);
                    list.Add(item);
                }
                _requestHistoryView.FillRequestList(list.ToArray());
            }
        }
    }
}