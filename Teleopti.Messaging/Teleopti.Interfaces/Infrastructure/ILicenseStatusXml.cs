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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		XmlDocument GetNewStatusDocument();

        ///<summary>
        ///</summary>
        bool AlmostTooMany { get; set; }

        ///<summary>
        ///</summary>
        int NumberOfActiveAgents { get; set; }

        ///<summary>
        ///</summary>
        int DaysLeft { get; set; }
    }
}