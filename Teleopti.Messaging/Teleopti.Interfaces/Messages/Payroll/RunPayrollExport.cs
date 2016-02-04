using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Messages.Payroll
{
    /// <summary>
    /// Message to inform consumers that a payroll export shoud be run
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2010-11-22
    /// </remarks>
    public class RunPayrollExport : MessageWithLogOnContext
    {

        /// <summary>
        /// Creates a new instance of the message to initiate a new payroll export run
        /// </summary>
        public RunPayrollExport()
        {
            ExportPersonIdCollection = new Collection<Guid>();
        }

        /// <summary>
        /// Definies an identity for this message (typically the Id of the root this message refers to.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-11-22
        /// </remarks>
        public override Guid Identity
        {
            get { return PayrollExportId; }
        }

        /// <summary>
        /// Gets or sets the payroll export id.
        /// </summary>
        /// <value>The payroll export id.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-11-22
        /// </remarks>
        public Guid PayrollExportId { get; set; }

        /// <summary>
        /// Gets or sets the owner person id.
        /// </summary>
        /// <value>The owner person id.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2010-11-22
        /// </remarks>
        public Guid OwnerPersonId { get; set; }

        /// <summary>
        /// The period to run the payroll export for
        /// </summary>
        public DateOnlyPeriod ExportPeriod { get; set; }

        /// <summary>
        /// The output format id for this payroll export
        /// </summary>
        public Guid PayrollExportFormatId { get; set; }

        /// <summary>
        /// The id's of the people to include in this payroll export
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<Guid> ExportPersonIdCollection { get; set; }

        /// <summary>
        /// The id's of the payroll result
        /// </summary>
        public Guid PayrollResultId { get; set; }

    }
}