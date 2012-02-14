using System;
using System.ComponentModel;

namespace Teleopti.Ccc.AgentPortalCode.Common.Controls
{
    public interface IDateSelectorView
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        BindingList<DateTime> DateList { get; set;}
        DateTime CurrentDate { get; set; }
        DateTime CurrentDateFrom { get; set; }
        DateTime CurrentDateTo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        BindingList<DateTime> SelectedDeleteDates { get; set; }
        DateTime InitialDate { set; }
        void AddDate();
        void AddDateRange();
        void DeleteDates();
    }
}