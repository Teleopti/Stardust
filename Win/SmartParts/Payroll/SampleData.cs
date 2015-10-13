using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.WinCode.Payroll.PayrollExportPages.PayrollExportSmartPart;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.SmartParts.Payroll
{
    public static class SampleData
    {
        public static CollectionViewSource PayrollResults
        {
            get
            {
                var payrollResultsCollection = new ObservableCollection<PayrollResultViewModel>();
                var result1 = new PayrollResult(new PayrollExport {PayrollFormatName = "Name Here"}, new Person {Name = new Name("Junie", "Browning")}, DateTime.UtcNow);
                var result2 = new PayrollResult(new PayrollExport {PayrollFormatName = "Name Here"}, new Person {Name = new Name("Junie", "Browning")}, DateTime.UtcNow);
                var result3 = new PayrollResult(new PayrollExport {PayrollFormatName = "Name Here"}, new Person {Name = new Name("Junie", "Browning")}, DateTime.UtcNow);
                result1.AddDetail(new PayrollResultDetail(DetailLevel.Error, "Hell fire!!", DateTime.UtcNow,new Exception("Error")));
                result1.AddDetail(new PayrollResultDetail(DetailLevel.Error, "Hell fire!!", DateTime.UtcNow, new Exception("Error")));
                result1.AddDetail(new PayrollResultDetail(DetailLevel.Error, "Hell fire!!", DateTime.UtcNow, new Exception("Error")));
                result1.AddDetail(new PayrollResultDetail(DetailLevel.Error, "Hell fire!!", DateTime.UtcNow, new Exception("Error")));
                result1.AddDetail(new PayrollResultDetail(DetailLevel.Error, "Hell fire!!", DateTime.UtcNow, new Exception("Error")));
                result1.AddDetail(new PayrollResultDetail(DetailLevel.Error, "Hell fire!!", DateTime.UtcNow, new Exception("Error")));
                result1.AddDetail(new PayrollResultDetail(DetailLevel.Error, "Hell fire!!", DateTime.UtcNow, new Exception("Error")));
                result1.AddDetail(new PayrollResultDetail(DetailLevel.Error, "Hell fire!!", DateTime.UtcNow, new Exception("Error")));
                payrollResultsCollection.Add(new PayrollResultViewModel(result1));
                payrollResultsCollection.Add(new PayrollResultViewModel(result2));
                payrollResultsCollection.Add(new PayrollResultViewModel(result3));
               
                return new CollectionViewSource {Source = payrollResultsCollection};
            }
        }

        public static ExportStatus Status
        {
            get { return new StatusDone(); }
        }

        public static JobResultProgress Progress
        {
            get { return new JobResultProgress {Message = "Error in file", Percentage = 42};}
        }
    }
}
