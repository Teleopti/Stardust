using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// A command that sets values to the optional columns displayed in people. 
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2013/03/")]
    public class SetPersonOptionalValuesForPersonCommandDto : CommandDto
    {
        private ICollection<OptionalValueDto> _optionalValueCollection;

        public SetPersonOptionalValuesForPersonCommandDto()
        {
            OptionalValueCollection = new Collection<OptionalValueDto>();
        }

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// The list of key/value pairs to apply as values to the columns.
        /// </summary>
        /// <remarks>
        /// The user running this command must have permission to work with the target person in people.
        /// If there's no existing optional column with the given name an exception will be thrown.
        /// </remarks>
        [DataMember]
        public ICollection<OptionalValueDto> OptionalValueCollection
        {
            get { return _optionalValueCollection; }
            private set
            {
                if (value!=null)
                {
                    _optionalValueCollection = new List<OptionalValueDto>(value);
                }
            }
        }
    }
}