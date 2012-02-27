using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security
{
    public class LicenseStatus : AggregateRoot, ILicenseStatus 
    {
        private string _xmlString;

        public virtual string XmlString
        {
            get { return _xmlString; }
            set { _xmlString = value; }
        }

    }
}
