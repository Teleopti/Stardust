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

        public XmlResult(IPayrollResult payrollResult) : this()
        {
            SetParent(payrollResult);
        }

        public virtual IXPathNavigable XPathNavigable
        {
	        get
	        {
		        if (!string.IsNullOrEmpty(_xPathNavigable))
				{
					var doc = new XmlDocument();
			        doc.LoadXml(_xPathNavigable);
					return doc;
		        }
		        return null;
	        }
        }

        public virtual void SetResult(IXPathNavigable xmlResult)
        {
            _xPathNavigable = xmlResult.CreateNavigator().OuterXml;
            ((IPayrollResult) Parent).FinishedOk = true;
        }
    }
}
