using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class PersonRequestReadOnlyRepository : IPersonRequestReadOnlyRepository
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public PersonRequestReadOnlyRepository(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }
        public IList<IPersonRequestLightWeight> LoadOnPerson(Guid personId, int startRow, int endRow)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                return ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(
                    "exec [ReadModel].[RequestOnPerson] @person=:person, @start_row =:start_row, @end_row=:end_row")
                    .SetGuid("person", personId)
                    .SetInt32("start_row", startRow)
                    .SetInt32("end_row", endRow)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(PersonRequestLightWeight)))
                    .SetReadOnly(true)
                    .List<IPersonRequestLightWeight>();
            }  
        }
    }

    public class PersonRequestLightWeight: IPersonRequestLightWeight
    {
        public Guid Id { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmplyomentNumber { get; set; }
        public int RequestStatus { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string DenyReason { get; set; }
        public string Info { get; set; }
        public string RequestType { get; set; }
        public int ShiftTradeStatus { get; set; }
        public string SavedByFirstName { get; set; }
        public string SavedByLastName { get; set; }
        public string SavedByEmplyomentNumber { get; set; }
    }
}