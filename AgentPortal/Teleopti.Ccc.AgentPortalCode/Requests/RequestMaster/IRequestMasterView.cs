using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.AgentPortalCode.Requests.RequestMaster
{
    public interface IRequestMasterView
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IList<RequestDetailRow> DataSource { get; set; }
        string MessageHeader { get; set; }
        string RequestDateHeader { get; set; }
        string RequestTypeHeader { get; set; }
        string RequestStatusHeader { get; set; }
        string DetailsHeader { get; set; }
        string SubjectHeader { get; set; }
        string LastChangedHeader { get; set; }
        int CurrentSelectedRow { get; set; }
    }
}
