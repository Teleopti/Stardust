using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class PersonFinderReadOnlyRepository : IPersonFinderReadOnlyRepository
    {
        private readonly ICurrentUnitOfWork _currentUnitOfWork;

        public PersonFinderReadOnlyRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            _currentUnitOfWork = currentUnitOfWork;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider",
            MessageId = "System.String.Format(System.String,System.Object[])"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
             "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Find(IPersonFinderSearchCriteria personFinderSearchCriteria)
        {
            personFinderSearchCriteria.TotalRows = 0;
            int cultureId = Domain.Security.Principal.TeleoptiPrincipal.Current.Regional.UICulture.LCID;
	        if (personFinderSearchCriteria.TerminalDate < new DateOnly(1753, 1, 1))
		        personFinderSearchCriteria.TerminalDate = new DateOnly(1753, 1, 1);

            var uow = _currentUnitOfWork.Current();
            var result = ((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
                "exec [ReadModel].PersonFinder @search_string=:search_string, @search_type=:search_type, @leave_after=:leave_after, @start_row =:start_row, @end_row=:end_row, @order_by=:order_by, @sort_direction=:sort_direction, @culture=:culture")
                                                     .SetString("search_string",
                                                                personFinderSearchCriteria.SearchValue)
                                                     .SetString("search_type",
                                                                Enum.GetName(typeof (PersonFinderField),
                                                                             personFinderSearchCriteria.Field))
                                                     .SetDateTime("leave_after",
                                                                  personFinderSearchCriteria.TerminalDate)
                                                     .SetInt32("start_row", personFinderSearchCriteria.StartRow)
                                                     .SetInt32("end_row", personFinderSearchCriteria.EndRow)
                                                     .SetInt32("order_by", personFinderSearchCriteria.SortColumn)
                                                     .SetInt32("sort_direction",
                                                               personFinderSearchCriteria.SortDirection)
                                                     .SetInt32("culture", cultureId)
                                                     .SetResultTransformer(
                                                         Transformers.AliasToBean(typeof (PersonFinderDisplayRow)))
                                                     .SetReadOnly(true)
                                                     .List<IPersonFinderDisplayRow>();

            int row = 0;
            foreach (var personFinderDisplayRow in result)
            {
                personFinderSearchCriteria.TotalRows = personFinderDisplayRow.TotalCount;
                personFinderSearchCriteria.SetRow(row, personFinderDisplayRow);
                row++;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider",
            MessageId = "System.String.Format(System.String,System.Object)")]
        public void UpdateFindPerson(ICollection<Guid> ids)
        {
            string inputIds = String.Join(",", (from p in ids select p.ToString()).ToArray());
            var uow = _currentUnitOfWork.Current();

            ((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
                string.Format("exec [ReadModel].[UpdateFindPerson] '{0}'", inputIds)).ExecuteUpdate();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider",
            MessageId = "System.String.Format(System.String,System.Object)")]
        public void UpdateFindPersonData(ICollection<Guid> ids)
        {
            string inputIds = String.Join(",", (from p in ids select p.ToString()).ToArray());
            var uow = _currentUnitOfWork.Current();
            ((NHibernateUnitOfWork) uow).Session.CreateSQLQuery(
                string.Format("exec [ReadModel].[UpdateFindPersonData] '{0}'", inputIds)).ExecuteUpdate();
        }
    }

    public class PersonFinderDisplayRow : IPersonFinderDisplayRow
    {
        public Guid PersonId { get; set; }

        public Guid? TeamId { get; set; }

        public Guid? SiteId { get; set; }

        public Guid BusinessUnitId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmploymentNumber { get; set; }
        public string Note { get; set; }
        public DateTime TerminalDate { get; set; }
        public bool Grayed { get; set; }
        public int TotalCount { get; set; }
        public Int64 RowNumber { get; set; }
    }
}
