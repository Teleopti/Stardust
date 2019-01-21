using System;
using System.Collections.Generic;
using System.Security.Claims;
using NHibernate.Impl;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.Util;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

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
			_unitOfWork = new ThisUnitOfWork(unitOfWork);
		}

		private Guid getBusinessUnitId()
		{
			var filter = (FilterImpl)_unitOfWork.Session().GetEnabledFilter("businessUnitFilter");
			object businessUnitId;
			if (!filter.Parameters.TryGetValue("businessUnitParameter", out businessUnitId))
			{
				return ((ITeleoptiIdentity)ClaimsPrincipal.Current.Identity).BusinessUnitId.GetValueOrDefault();
			}
			return Guid.Parse(businessUnitId.ToString());
		}

		public IList<IPersonAuthorization> GetPersonFor(DateOnly shiftTradeDate, IList<Guid> groupIdList , string name, NameFormatSetting nameFormat = NameFormatSetting.FirstNameThenLastName)
		{
		    var useChineseNameFormat = name != null && StringHelper.StringContainsChinese(name);
		    var firstNameFirst = nameFormat == NameFormatSetting.FirstNameThenLastName;

			const string sql = "exec ReadModel.LoadPersonForScheduleSearch @scheduleDate=:scheduleDate, "
							   + "@groupIdList=:groupIdList, @businessUnitId=:businessUnitId, @name=:name, "
                               + "@noSpaceInName = :noSpaceInName, "
                               + "@firstNameFirst = :firstNameFirst";
			var result = new List<IPersonAuthorization>();
			groupIdList.Batch(100).ForEach(l =>
			{
				var batchResult = ((NHibernateUnitOfWork)_unitOfWork.Current()).Session.CreateSQLQuery(sql)
				.SetDateOnly("scheduleDate", shiftTradeDate)
				.SetString("groupIdList", string.Join(",", l))
				.SetString("name", name)
				.SetBoolean("noSpaceInName", useChineseNameFormat)
				.SetBoolean("firstNameFirst", firstNameFirst)
				.SetGuid("businessUnitId", getBusinessUnitId())
				.SetResultTransformer(Transformers.AliasToBean(typeof(PersonSelectorShiftTrade)))
				.SetReadOnly(true)
				.List<IPersonAuthorization>();
				result.AddRange(batchResult);
			});
			return result;
		}
	}

	public class PersonSelectorShiftTrade : IPersonAuthorization
	{
		public Guid PersonId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}
}