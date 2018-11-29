using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages.PayrollExportSmartPart;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Payroll
{
    public static class SampleData
    {
        public static CollectionViewSource PayrollResults
        {
            get
            {
                var payrollResultsCollection = new ObservableCollection<PayrollResultViewModel>();
	            var person1 = new Person();
				var person2 = new Person();
				var person3 = new Person();
				person1.SetName(new Name("Junie", "Browning"));
				person2.SetName(new Name("Junie", "Browning"));
				person3.SetName(new Name("Junie", "Browning"));
				var result1 = new PayrollResult(new PayrollExport {PayrollFormatName = "Name Here"}, person1, DateTime.UtcNow);
				var result2 = new PayrollResult(new PayrollExport {PayrollFormatName = "Name Here"}, person2, DateTime.UtcNow);
	            var result3 = new PayrollResult(new PayrollExport {PayrollFormatName = "Name Here"}, person3, DateTime.UtcNow);
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
