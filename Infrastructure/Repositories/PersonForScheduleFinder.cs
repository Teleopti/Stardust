using System;
using System.Collections.Generic;
using System.Security.Claims;
using NHibernate.Impl;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonForScheduleFinder : IPersonForScheduleFinder
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PersonForScheduleFinder(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public PersonForScheduleFinder(IUnitOfWork unitOfWork)
		{
			_unitOfWork = new FixedCurrentUnitOfWork(unitOfWork);
		}

		private Guid getBusinessUnitId()
		{
			var filter = (FilterImpl)_unitOfWork.Session().GetEnabledFilter("businessUnitFilter");
			object businessUnitId;
			if (!filter.Parameters.TryGetValue("businessUnitParameter", out businessUnitId))
			{
				businessUnitId = ((ITeleoptiIdentity)ClaimsPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault();
			}
			return Guid.Parse(businessUnitId.ToString());
		}

		public IList<IAuthorizeOrganisationDetail> GetPersonFor(DateOnly shiftTradeDate, IList<Guid> groupIdList , string name)
		{
			const string sql = "exec ReadModel.LoadPersonForScheduleSearch @scheduleDate=:scheduleDate, "
							   + "@groupIdList=:groupIdList, @businessUnitId=:businessUnitId, @name=:name ";
			return ((NHibernateUnitOfWork) _unitOfWork.Current()).Session.CreateSQLQuery(sql)
				.SetDateOnly("scheduleDate", shiftTradeDate)
				.SetString("groupIdList", string.Join(",", groupIdList))
				.SetString("name", name)
				.SetGuid("businessUnitId", getBusinessUnitId())
				.SetResultTransformer(Transformers.AliasToBean(typeof (PersonSelectorShiftTrade)))
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