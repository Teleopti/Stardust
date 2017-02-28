using System.Drawing;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Absence
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-08-07
    /// </remarks>
    public interface IAbsence : IPayload, ICloneableEntity<IAbsence>
                                    
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        Description Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the display color of the Payload.
        /// </summary>
        /// <value>The color of the display.</value>
        /// <remarks>No property later - methods instead!</remarks>
        Color DisplayColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the priority.
        /// Used if a person has multiple PersonAbsences
        /// </summary>
        /// <value>The priority.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-05
        /// </remarks>
        byte Priority { get; set; }

        /// <summary>
        /// Gets the name part of the description.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-10-02
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IAbsence"/> is requestable from preferences.
        /// </summary>
        /// <value><c>true</c> if requestable; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-03
        /// </remarks>
        bool Requestable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Absence is calculated as work time.
        /// </summary>
        /// <value><c>true</c> if [work time]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-11-20
        /// </remarks>
        bool InWorkTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Absence is calculated as paid time.
        /// </summary>
        /// <value><c>true</c> if [paid time]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-11-20
        /// </remarks>
        bool InPaidTime { get; set; }


        /// <summary>
        /// Gets or sets the payroll code taken from the payrollsystem.
        /// </summary>
        /// <value>The payroll code.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2009-03-03
        /// </remarks>
        string PayrollCode{get;set;}

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IAbsence"/> is confidential
        /// or visible in projection for everyone.
        /// </summary>
        /// <value><c>true</c> if confidential; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-02-09
        /// </remarks>
        bool Confidential { get; set; }
    }
}
