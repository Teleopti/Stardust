using System;
using System.ComponentModel;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade;

namespace Teleopti.Ccc.AgentPortalCode.Common.Controls
{
    public class DateSelectorPresenter
    {
        private readonly ShiftTradeModel _model;
        private readonly IDateSelectorView _view;

        public DateSelectorPresenter(ShiftTradeModel model, IDateSelectorView view)
        {
            _model = model;
            _view = view;
        }

        public void Initialize()
        {
            _view.InitialDate = _model.InitialDate;
            _view.DateList = _model.TradeDates;
            _view.SelectedDeleteDates = _model.DeletedDates;
        }

        public void UpdateData()
        {
            _model.DeletedDates = _view.SelectedDeleteDates;
        }

        public void AddDate()
        {
            if (!alreadyExistsInList(_view.CurrentDate))
            {
                _model.AddCurrentSelectedDate(_view.CurrentDate);
            }
        }

        public void AddDateRange()
        {
            DateTime dateTime = _view.CurrentDateFrom;
            while (dateTime <= _view.CurrentDateTo)
            {
                if (!alreadyExistsInList(dateTime))
                    _model.AddCurrentSelectedDate(dateTime);
                dateTime = dateTime.AddDays(1);
            }
        }

        private bool alreadyExistsInList(DateTime dateTime)
        {
            foreach (DateTime model in _model.TradeDates)
            {
                if (model == dateTime)
                {
                    return true;
                }
            }
            return false;
        }

        public void DeleteDates()
        {
            UpdateData();
            foreach (DateTime deletedDate in _model.DeletedDates)
            {
                _model.TradeDates.Remove(deletedDate);
            }
        }

        public BindingList<DateTime> GetSelectedItems(ListBox.SelectedObjectCollection selectedItems, BindingList<DateTime> deletedList)
        {
            deletedList.Clear();
            foreach (DateTime dateTime in selectedItems)
            {
                deletedList.Add(dateTime);
            }
            return deletedList;
        }
    }
}