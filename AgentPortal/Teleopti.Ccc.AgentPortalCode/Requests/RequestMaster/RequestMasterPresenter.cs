using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Requests.RequestMaster
{
    public class RequestMasterPresenter
    {
        private readonly IRequestMasterView _view;
        private readonly RequestMasterModel _model;
        private string _sortMember;
        private ListSortDirection _order;

        public RequestMasterPresenter(IRequestMasterView view, RequestMasterModel model)
        {
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            LoadDataSource();
            _view.DataSource = _model.DataSource;
            _view.RequestDateHeader = _model.RequestDateHeader;
            _view.RequestTypeHeader = _model.RequestTypeHeader;
            _view.RequestStatusHeader = _model.RequestStatusHeader;
            _view.DetailsHeader = _model.DetailsHeader;
            _view.SubjectHeader = _model.SubjectHeader;
            _view.MessageHeader = _model.MessageHeader;
            _view.LastChangedHeader = _model.LastChangedHeader;
            SortByColumn("LastChanged", ListSortDirection.Descending);
        }

        //Virtual grid cant handle sorting of the columns automatically, had to do our own,?!!? 2009?
        public void SortByColumn(string sortMember, ListSortDirection order)
        {
            //Store the sortMember
            _sortMember = sortMember;
            _order = order;
            //Just add the items in a sorted list and after that take'em out forward or backwards
            //Hope the speed will hold.
            bool sortingOk;

            SortedList<object, RequestDetailRow> sortedList = getSortedList(sortMember, out sortingOk);

            if (!sortingOk) return;

            checkDirection(sortedList, order);

            _view.DataSource = _model.DataSource;
        }

        private SortedList<object, RequestDetailRow> getSortedList(string sortMember, out bool sortingOk)
        {
            sortingOk = true;
            SortedList<object, RequestDetailRow> sortedList = new SortedList<object, RequestDetailRow>();
            int eq = 1; //If 2 items are exactly the same
            foreach (RequestDetailRow row in _model.DataSource)
            {
                PropertyInfo propertyInfo = row.GetType().GetProperty(sortMember);
                if (propertyInfo != null)
                {
                    string value;
                    
                    if (isDate(row, propertyInfo))
                        value = string.Format(CultureInfo.GetCultureInfo("sv-SE"), "{0}{1}", propertyInfo.GetValue(row, null), eq);
                    else    
                        value = string.Format(CultureInfo.CurrentUICulture, "{0}{1}", propertyInfo.GetValue(row, null), eq);
                    
                    sortedList.Add(value, row);
                }
                else
                {
                    sortingOk = false;
                    break;
                }
                eq++;
            }
            return sortedList;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private bool isDate(RequestDetailRow row, PropertyInfo propertyInfo)
        {
            //Ugly stuff, but I got to do it
            return typeof(DateTime).Equals(propertyInfo.GetValue(row, null).GetType());
        }

        private void checkDirection(SortedList<object, RequestDetailRow> sortedList, ListSortDirection order)
        {
            if (order == ListSortDirection.Ascending)
            {
                _model.DataSource = sortedList.Values;
            }
            else
            {
                List<RequestDetailRow> reverse = new List<RequestDetailRow>(sortedList.Values);
                reverse.Reverse();
                _model.DataSource = reverse;
            }
        }

        public void LoadDataSource()
        {
            ICollection<PersonRequestDto> personRequestDtoCollection = _model.SdkService.GetAllRequestModifiedWithinPeriodOrPending(
                                                                                    _model.LoggedOnPerson,
                                                                                    DateTime.UtcNow.Date.AddDays(-180),
                                                                                    true,
                                                                                    DateTime.SpecifyKind(DateTime.MaxValue.Date,DateTimeKind.Utc), 
                                                                                    true);
            var detailList = new List<RequestDetailRow>();
            foreach (var dto in personRequestDtoCollection)
            {
                detailList.Add(new RequestDetailRow(dto,_model.LoggedOnPerson));
            }
            _model.DataSource = detailList;
            _view.DataSource = _model.DataSource;
            keepSortOrder();
        }

        private void keepSortOrder()
        {
            if(_sortMember!=null)
                SortByColumn(_sortMember,_order);
        }

        public void DeletePersonRequests(IList<RequestDetailRow> requestDetailRows)
        {
            foreach (RequestDetailRow requestDetailRow in requestDetailRows)
            {
                if (requestDetailRow.CanDelete)
                    _model.SdkService.DeletePersonRequest(requestDetailRow.PersonRequest);    
            }
            LoadDataSource();
            _view.DataSource = _model.DataSource;
        }
    }
}
