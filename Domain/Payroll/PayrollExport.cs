using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Payroll
{
    public class PayrollExport : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IPayrollExport, IDeleteTag
    {
        private string _name;
        private IList<IPerson> _persons = new List<IPerson>();
        private DateOnlyPeriod _period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1));
        private Guid _payrollFormatId;
        private string _payrollFormatName;
        private ExportFormat _fileFormat;
        private bool _isDeleted;

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the payroll format id.
        /// </summary>
        /// <value>The payroll format id.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2009-02-27
        /// </remarks>
        public virtual Guid PayrollFormatId

        {
            get { return _payrollFormatId; }
            set { _payrollFormatId = value; }
        }

        /// <summary>
        /// Gets or sets the name of the payroll format.
        /// </summary>
        /// <value>The name of the payroll format.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2009-02-27
        /// </remarks>
        public virtual string PayrollFormatName
        {
            get { return _payrollFormatName; }
            set { _payrollFormatName = value; }
        }


        public virtual ExportFormat FileFormat
        {
            get { return _fileFormat; }
            set { _fileFormat = value; }
        }

        public virtual DateOnlyPeriod Period
        {
            get { return _period; }
            set
            {
                _period = value;
            }
        }
		
        public virtual void ClearPersons()
        {
            _persons.Clear();
        }

        public virtual void AddPersons(IEnumerable<IPerson> persons)
        {
            foreach (IPerson person in persons)
            {
                _persons.Add(person);
            }
        }

        public virtual ReadOnlyCollection<IPerson> Persons => new ReadOnlyCollection<IPerson>(_persons);

	    public virtual bool IsDeleted => _isDeleted;

	    public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

				public virtual IPerson CreatedBy { get; protected set; }
				public virtual DateTime? CreatedOn { get; protected set; }

    }
}
