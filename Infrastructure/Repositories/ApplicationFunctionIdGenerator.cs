using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Id;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class ApplicationFunctionIdGenerator : IIdentifierGenerator
    {
        #region IIdentifierGenerator Members

        /// <summary>
        /// Generates the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public object Generate(NHibernate.Engine.ISessionImplementor session, object obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
