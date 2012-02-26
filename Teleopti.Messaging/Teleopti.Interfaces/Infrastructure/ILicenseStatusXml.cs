using System;
using System.Xml;

namespace Teleopti.Interfaces.Infrastructure
{
    ///<summary>
    ///</summary>
    public interface ILicenseStatusXml
    {
        ///<summary>
        ///</summary>
        DateTime CheckDate { get; set; }
        ///<summary>
        ///</summary>
        bool StatusOk { get; set; }
        ///<summary>
        ///</summary>
        DateTime LastValidDate { get; set; }
        ///<summary>
        ///</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        XmlDocument XmlDocument { get; }

        ///<summary>
        ///</summary>
        bool AlmostTooMany { get; set; }
    }
}