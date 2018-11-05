﻿using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	internal static class UnitOfWorkExtensions
	{
		internal static ISession Session(this ICurrentUnitOfWork unitOfWork)
		{
			return unitOfWork.Current().Session();
		}

		internal static ISession Session(this IUnitOfWork unitOfWork)
		{
			return (unitOfWork as IHaveSession).GetSession();
		}
	}
}