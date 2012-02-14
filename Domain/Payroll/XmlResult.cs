using System.Xml.XPath;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Payroll
{
    public class XmlResult : AggregateEntity, IXmlResult
    {
        private IXPathNavigable _xPathNavigable;

        protected XmlResult()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public XmlResult(IPayrollResult payrollResult) : this()
        {
            SetParent(payrollResult);
        }

        public virtual IXPathNavigable XPathNavigable
        {
            get { return _xPathNavigable; }
        }

        public virtual void AddResult(IXPathNavigable xmlResult)
        {
            _xPathNavigable = xmlResult;
            ((IPayrollResult) Parent).FinishedOk = true;
        }
    }
}
