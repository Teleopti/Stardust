using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Class that holds a list with interval information of adherence data 
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class AdherenceDto
    {
        private ICollection<AdherenceDataDto> _adherenceDataDtos = new List<AdherenceDataDto>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), DataMember]
        public ICollection<AdherenceDataDto> AdherenceDataDtos
        {
            get { return _adherenceDataDtos; }
            set { _adherenceDataDtos = value; }
        }

    }
}
