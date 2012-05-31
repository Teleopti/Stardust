﻿using System;
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
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        
        public PersonFinderReadOnlyRepository(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }

         [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
         public void Find(IPersonFinderSearchCriteria personFinderSearchCriteria)
         {
             personFinderSearchCriteria.TotalRows = 0;
             int cultureId = Domain.Security.Principal.TeleoptiPrincipal.Current.Regional.UICulture.LCID;

             using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
             {
                 var result = ((NHibernateStatelessUnitOfWork) uow).Session.CreateSQLQuery(
                     "exec [ReadModel].PersonFinder @search_string=:search_string, @search_type=:search_type, @leave_after=:leave_after, @start_row =:start_row, @end_row=:end_row, @order_by=:order_by, @sort_direction=:sort_direction, @culture=:culture")
                     .SetString("search_string", personFinderSearchCriteria.SearchValue)
                     .SetString("search_type", Enum.GetName(typeof(PersonFinderField), personFinderSearchCriteria.Field))
                     .SetDateTime("leave_after", personFinderSearchCriteria.TerminalDate)
                     .SetInt32("start_row", personFinderSearchCriteria.StartRow)
                     .SetInt32("end_row", personFinderSearchCriteria.EndRow)
                     .SetInt32("order_by", personFinderSearchCriteria.SortColumn)
                     .SetInt32("sort_direction", personFinderSearchCriteria.SortDirection)
                     .SetInt32("culture", cultureId)
                     .SetResultTransformer(Transformers.AliasToBean(typeof (PersonFinderDisplayRow)))
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
         }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        public  void UpdateFindPerson(Guid[] inputIds )
        {
            string ids = String.Join(",", (from p in inputIds select p.ToString()).ToArray());
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(
                    string.Format("exec [ReadModel].[UpdateFindPerson] '{0}'", ids)).ExecuteUpdate();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        public void UpdateFindPersonData(Guid[] inputIds)
        {
            string ids = String.Join(",", (from p in inputIds select p.ToString()).ToArray());
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(
                    string.Format("exec [ReadModel].[UpdateFindPersonData] '{0}'", ids)).ExecuteUpdate();
            }
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
