using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages.PayrollExportSmartPart;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Payroll.PayrollExportSmartPart
{
    [TestFixture]
    public class PayrollExportSmartPartViewModelTest : SmartClientPortal.Shell.WinCode.Common.IObservable<IPayrollResult>, IDisposable
    {
        private PayrollExportSmartPartViewModel _target;
        private IPayrollResult _resultToSave; //Called by IObservable

        [SetUp]
        public void Setup()
        {
            _target = new PayrollExportSmartPartViewModel(this);
        }
	
        [Test]
        public void VerifyCanLoadPayrollResults()
        {
            ICollection<IPayrollResult> results = new List<IPayrollResult>();
            var first = new PayrollResult(new PayrollExport(), new Person(), new DateTime().AddDays(1));
            var second = new PayrollResult(new PayrollExport(), new Person(), new DateTime());

            results.Add(first);
            results.Add(second);

            _target.LoadPayrollResults(results);
            Assert.AreEqual(2, _target.PayrollResultsCollection.Count);

            //Assert is cleared
            _target.LoadPayrollResults(results);
            Assert.AreEqual(2, _target.PayrollResultsCollection.Count);
        }

        [Test]
        public void VerifyCallsNotifyWithSelectedPayroll()
        {
            
            var models = new TesterForCommandModels();
            //Check that we can add add:
            var args = models.CreateCanExecuteRoutedEventArgs();
            _target.SaveAs.OnQueryEnabled(null, args);
            Assert.IsFalse(args.CanExecute,"Cannot execute SaveAs, no PayrollResult selected");

            var result1 = new PayrollResult(new PayrollExport(), new Person(), new DateTime().AddDays(1));
            var result2 = new PayrollResult(new PayrollExport(), new Person(), new DateTime());

            ICollection<IPayrollResult> results = new List<IPayrollResult>();
            results.Add(result1);
            results.Add(result2);
            _target.LoadPayrollResults(results);

            result1.XmlResult.SetResult(new FakeXml());
            
            _target.PayrollResults.View.MoveCurrentToFirst(); //same as selecting in gui
            _target.SaveAs.OnQueryEnabled(null, args);          //Check that command is enabled
            Assert.IsTrue(args.CanExecute,"Can execute SaveAs, first PayrollResult selected");
            
            //Execute SaveAs-Command:
            var executeArgs = models.CreateExecutedRoutedEventArgs();
            _target.SaveAs.OnExecute(null, executeArgs);

            Assert.AreEqual(_resultToSave,result1,"When SaveAs is executed, Notify  is calleed with seleceted PayrollResult (result1)");



            _target.PayrollResults.View.MoveCurrentToPosition(1); //same as selecting in gui
            //Execute SaveAs-Command:
            _target.SaveAs.OnExecute(null, executeArgs);
            Assert.AreEqual(_resultToSave, result2, "When SaveAs is executed, Notify  is calleed with seleceted PayrollResult (result2)");
        }

        [Test]
        public void VerifySelectedPayrollDoNotObserverIfNull()
        {
            _target.Dispose();
            _target = new PayrollExportSmartPartViewModel();
            var result1 = new PayrollResult(new PayrollExport(), new Person(), new DateTime().AddDays(1));
            var result2 = new PayrollResult(new PayrollExport(), new Person(), new DateTime());
            ICollection<IPayrollResult> results = new List<IPayrollResult>();
            results.Add(result1);
            results.Add(result2);
         
            _target.LoadPayrollResults(results);
            _resultToSave = null;
            _target.PayrollResults.View.MoveCurrentTo(result1);
           
            Assert.IsNull(_resultToSave);
        }

        [Test]
        public void ShouldHaveCorrectTextOnSaveCommand()
        {
            Assert.AreEqual(UserTexts.Resources.SaveAs,_target.SaveAs.Text);
        }

        [Test]
        public void ShouldReportProgress()
        {
            IPayrollResult result1 = new PayrollResult(new PayrollExport(), new Person(), new DateTime());
            result1.SetId(Guid.NewGuid());

            var results = new List<IPayrollResult> {result1};
            _target.LoadPayrollResults(results);

            _target.UpdateProgress(new JobResultProgress{Message = "test",JobResultId = result1.Id.GetValueOrDefault(), Percentage = 33});

            Assert.AreEqual(33, _target.PayrollResultsCollection[0].Progress.Percentage);
        }

        public void Notify(IPayrollResult item)
        {
            _resultToSave = item;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();
            }
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
            _target.Dispose();
        }
    }

    public class FakeXml: IXPathNavigable
    {
        public XPathNavigator CreateNavigator()
        {
	        var document = new XmlDocument();
	        return document.CreateNavigator();
        }
    }
}
