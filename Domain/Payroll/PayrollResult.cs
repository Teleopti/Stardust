using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Payroll
{
    public class PayrollResult : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IPayrollResult
    {
        private readonly IPerson _owner;
        private readonly DateTime _timestamp;
        private readonly Guid _payrollFormatId;
        private readonly string _payrollFormatName;
        private readonly DateOnlyPeriod _period;
        private IXmlResult _xmlResult;
        private IPayrollExport _payrollExport;
        private readonly IList<IPayrollResultDetail> _details = new List<IPayrollResultDetail>();
        private bool _finishedOk;

        protected PayrollResult(){}

        public PayrollResult(IPayrollExport export, IPerson owner, DateTime timestamp)
        {
            _payrollFormatId = export.PayrollFormatId;
            _payrollFormatName = export.PayrollFormatName;
            _period = export.Period;
            _owner = owner;
            _timestamp = timestamp;
            _xmlResult = new XmlResult(this);
        }

        public virtual DateOnlyPeriod Period
        {
            get { return _period; }
        }

        public virtual string PayrollFormatName
        {
            get { return _payrollFormatName; }
        }

        public virtual Guid PayrollFormatId
        {
            get { return _payrollFormatId; }
        }

        public virtual DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public virtual IPerson Owner
        {
            get { return _owner; }
        }

        public virtual IXmlResult XmlResult
        {
            get { return _xmlResult; }
        }

        public virtual IPayrollExport PayrollExport
        {
            get { return _payrollExport; }
            set { _payrollExport = value; }
        }

        public virtual IEnumerable<IPayrollResultDetail> Details
        {
            get { return _details; }
        }

        public virtual void AddDetail(IPayrollResultDetail payrollResultDetail)
        {
            payrollResultDetail.SetParent(this);
            _details.Add(payrollResultDetail);
        }

        public virtual bool HasError()
        {
            return DetailHasError() || ResultTimeOut();
        }

        public virtual bool IsWorking()
        {
            return !_finishedOk && !HasError();
        }

        public virtual bool FinishedOk
        {
            get { return _finishedOk; }
            set { _finishedOk = value; }
        }

        private bool ResultTimeOut()
        {
            return !_finishedOk && _timestamp < DateTime.UtcNow.AddHours(-12);
        }

        private bool DetailHasError()
        {
            return _details.Any(d => d.DetailLevel == DetailLevel.Error);
        }
    }
}