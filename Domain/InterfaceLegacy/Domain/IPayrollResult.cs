using System;
using System.Collections.Generic;
using System.Xml.XPath;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Contains information about a payroll export made at a certain time
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-03-17
    /// </remarks>
    public interface IPayrollResult : IAggregateRoot
    {
        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-03-17
        /// </remarks>
        DateOnlyPeriod Period { get; }

        /// <summary>
        /// Gets the name of the payroll format.
        /// </summary>
        /// <value>The name of the payroll format.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-03-17
        /// </remarks>
        string PayrollFormatName { get; }

        /// <summary>
        /// Gets the payroll format id.
        /// </summary>
        /// <value>The payroll format id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-03-17
        /// </remarks>
        Guid PayrollFormatId { get; }

        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        /// <value>The timestamp.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-03-17
        /// </remarks>
        DateTime Timestamp { get; }

        /// <summary>
        /// Gets the owner.
        /// </summary>
        /// <value>The owner.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-03-17
        /// </remarks>
        IPerson Owner { get; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>The result.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-03-17
        /// </remarks>
        IXmlResult XmlResult { get; }

        /// <summary>
        /// Gets or sets the payroll export.
        /// </summary>
        /// <value>The payroll export.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-03-26
        /// </remarks>
        IPayrollExport PayrollExport { get; set; }

        ///<summary>
        /// Detailed information for this payroll result.
        ///</summary>
        IEnumerable<IPayrollResultDetail> Details { get; }


        ///<summary>
        /// Add detailed information about this payroll result (such as warnings, errors and other information).
        ///</summary>
        ///<param name="payrollResultDetail">The detailed information.</param>
        void AddDetail(IPayrollResultDetail payrollResultDetail);

        ///<summary>
        /// Check if this payroll result wasn't able to finish due to an error.
        ///</summary>
        ///<returns>True if an error occurred while running.</returns>
        bool HasError();

        ///<summary>
        /// Indicates if this payroll run is currently in progress.
        ///</summary>
        ///<returns>True if in progress.</returns>
        bool IsWorking();

        ///<summary>
        /// Inidicates if this payroll run is finished with no errors.
        ///</summary>
        bool FinishedOk { get; set; }
    }


    /// <summary>
    /// The Xml result for a payroll run.
    /// </summary>
    public interface IXmlResult
    {
        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>The result.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2011-02-09
        /// </remarks>
        IXPathNavigable XPathNavigable { get; }

        /// <summary>
        /// Adds the result.
        /// </summary>
        /// <param name="xmlResult">The xml result.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2011-02-09
        /// </remarks>
        void SetResult(IXPathNavigable xmlResult);
    }
}