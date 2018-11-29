using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Grouping
{
    public class FindPersonsModel : IFindPersonsModel
    {
        private readonly IEnumerable<IPerson> _persons;
        private DateOnlyPeriod _period;

        public FindPersonsModel(IEnumerable<IPerson> persons)
        {
            _persons = persons;
            _period = new DateOnlyPeriod(new DateOnly(DateTime.Today.AddMonths(-2)), new DateOnly(DateTime.Today.AddMonths(6)));
        }

        public DateOnly FromDate
        {
            get { return _period.StartDate; }
            set
            {
                _period = new DateOnlyPeriod(value, _period.EndDate);
            }
        }

        public DateOnly ToDate
        {
            get { return _period.EndDate; }
            set
            {
                _period = new DateOnlyPeriod(_period.StartDate, value);
            }
        }

        public IEnumerable<IPerson> Persons => _persons;
	}
}