using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a PayrollExportDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PayrollExportDto : Dto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayrollExportDto"/> class.
        /// </summary>
        public PayrollExportDto()
        {
            PersonCollection = new List<PersonDto>();
            ExportPersonCollection = new List<Guid>();
        }

        /// <summary>
        /// Gets or sets the time zone id. This defaults to the time zone of the person creating the export.
        /// </summary>
        /// <value>The time zone id.</value>
        [DataMember]
        public string TimeZoneId { get; set; }

        /// <summary>
        /// Gets or sets the person collection.
        /// </summary>
        /// <value>The person collection.</value>
        [DataMember]
        public IList<PersonDto> PersonCollection { get; private set; }

        /// <summary>
        /// The id's of people to include in the payroll export.
        /// </summary>
        [DataMember(IsRequired = false, Order = 1)]
        public IList<Guid> ExportPersonCollection { get; private set; }

        /// <summary>
        /// Gets or sets the period. This property is deprecated and will be removed. Use DatePeriod instead.
        /// </summary>
        /// <value>The period.</value>
        [DataMember]
        public DateTimePeriodDto Period { get; set; }

        /// <summary>
        /// Gets or sets the payroll format.
        /// </summary>
        /// <value>The payroll format.</value>
        [DataMember]
        public PayrollFormatDto PayrollFormat { get; set; }

        /// <summary>
        /// Gets or sets the date period. This one uses DateOnlyPeriod which is the preferred option.
        /// </summary>
        /// <value>The date period.</value>
        [DataMember]
        public DateOnlyPeriodDto DatePeriod { get; set; }

		/// <summary>
		/// Gets or sets the name for this payroll export.
		/// </summary>
		[DataMember(IsRequired = false, Order = 1)]
        public string Name { get; set; }
    }
}