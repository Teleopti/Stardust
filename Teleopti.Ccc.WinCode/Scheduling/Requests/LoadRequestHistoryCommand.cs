using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
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
                var requests = _repositoryFactory.CreateRequestHistoryReadOnlyRepository(uow).LoadOnPerson(selectedPerson, startRow, startRow + 50);
                //skapa listviewitems
                var list = new List<ListViewItem>();
                foreach (var request in requests)
                {
                    var item = new ListViewItem(request.RequestTypeText) { Tag = request };
                    item.SubItems.Add(request.RequestStatusText);
                    item.SubItems.Add(request.StartDateTime.ToShortDateString());
                    list.Add(item);
                }
                _requestHistoryView.FillRequestList(list.ToArray());
            }
        }
    }
}