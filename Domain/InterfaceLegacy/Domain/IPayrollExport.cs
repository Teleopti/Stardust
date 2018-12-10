using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Export of Payrolls.
    /// </summary>
    /// <remarks>
    /// Created by: HenryG
    /// Created date: 2009-02-24
    /// </remarks>
	public interface IPayrollExport : IAggregateRoot, IChangeInfo, ICreateInfo
    {

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-03-19
        /// </remarks>
        string Name
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the file format.
        /// </summary>
        /// <value>The file format.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-02-25
        /// </remarks>
        ExportFormat FileFormat
        {
            get; 
            set;
        }

        /// <summary>
        /// Gets the persons.
        /// </summary>
        /// <value>The persons.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-02-24
        /// </remarks>
        ReadOnlyCollection<IPerson> Persons
        {
            get;
        }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-02-25
        /// </remarks>
        DateOnlyPeriod Period
        {
            get; set;
        }


        /// <summary>
        /// Clears the persons.
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-02-27
        /// </remarks>
        void ClearPersons();

        /// <summary>
        /// Adds the persons.
        /// </summary>
        /// <param name="persons">The persons.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-02-27
        /// </remarks>
        void AddPersons(IEnumerable<IPerson> persons);

        /// <summary>
        /// Gets or sets the name of the payroll format.
        /// </summary>
        /// <value>The name of the payroll format.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2009-02-27
        /// </remarks>
        string PayrollFormatName
        {
            get; set;
        }
        /// <summary>
        /// Gets or sets the payroll format id.
        /// </summary>
        /// <value>The payroll format id.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2009-02-27
        /// </remarks>
        Guid PayrollFormatId
        {
            get; set;
        }
    }
}
