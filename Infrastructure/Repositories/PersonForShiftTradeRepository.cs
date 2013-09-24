using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonForShiftTradeRepository : IPersonForShiftTradeRepository
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PersonForShiftTradeRepository(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public PersonForShiftTradeRepository(IUnitOfWork unitOfWork)
		{
			_unitOfWork = new FixedCurrentUnitOfWork(unitOfWork);
		}

		public IList<IAuthorizeOrganisationDetail> GetPersonForShiftTrade(DateOnly shiftTradeDate, Guid? teamId)
		{
			return ((NHibernateUnitOfWork)_unitOfWork.Current()).Session.CreateSQLQuery(
				"exec ReadModel.LoadPersonForShiftTrade @shiftTradeDate=:shiftTradeDate, @myTeamId=:myTeamId")
			                                          .SetDateTime("shiftTradeDate", shiftTradeDate)
			                                          .SetParameter("myTeamId", teamId.HasValue ? teamId.Value : (Guid?)null)
			                                          .SetResultTransformer(Transformers.AliasToBean(typeof(PersonSelectorShiftTrade)))
			                                          .SetReadOnly(true)
			                                          .List<IAuthorizeOrganisationDetail>();
		}
	}

	public class PersonSelectorShiftTrade : IAuthorizeOrganisationDetail
	{
		public Guid PersonId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}
}