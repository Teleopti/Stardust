﻿using System;
using System.Collections.Generic;
using NHibernate;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Unitofwork factory for connections against Matrix
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-04-22
    /// </remarks>
    public class NHibernateUnitOfWorkMatrixFactory : NHibernateUnitOfWorkFactory
    {
        private const string notStatefulSupport = "This IUnitOfWorkFactory does not support stateful IUnitOfWorks";

        protected internal NHibernateUnitOfWorkMatrixFactory(ISessionFactory sessionFactory)
            : base(sessionFactory, null, new List<IMessageSender>())
        {
        }

        public override IUnitOfWork CreateAndOpenUnitOfWork()
        {
            throw new NotSupportedException(notStatefulSupport);
        }

    }

}
