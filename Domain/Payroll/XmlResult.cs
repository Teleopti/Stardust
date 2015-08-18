using System.Xml;
using System.Xml.XPath;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Payroll
{
    public class XmlResult : AggregateEntity, IXmlResult
    {
        private string _xPathNavigable;

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
	        get
	        {
		        var doc = new XmlDocument();
				doc.LoadXml(_xPathNavigable);
		        return doc;
	        }
        }

        public virtual void AddResult(IXPathNavigable xmlResult)
        {
            _xPathNavigable = xmlResult.CreateNavigator().OuterXml;
            ((IPayrollResult) Parent).FinishedOk = true;
        }
    }
}
