using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    ///</summary>
    public interface IAuthorizeOrganisationDetail
    {
        ///<summary>
        ///</summary>
        Guid PersonId { get; set; }
        ///<summary>
        ///</summary>
        Guid? TeamId { get; set; }
        ///<summary>
        ///</summary>
        Guid? SiteId { get; set; }
        ///<summary>
        ///</summary>
        Guid BusinessUnitId { get; set; }
    }
}