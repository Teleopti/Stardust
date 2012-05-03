using System;
using System.Runtime.Serialization;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [Serializable]
    public class ExplicitlySetGenericPrincipalDataSourceException : DataSourceException
    {
        private readonly GenericPrincipal _explicitPrincipal;

        public ExplicitlySetGenericPrincipalDataSourceException()
        {
        }

        public ExplicitlySetGenericPrincipalDataSourceException(string message)
            : base(message)
        {
        }

        public ExplicitlySetGenericPrincipalDataSourceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ExplicitlySetGenericPrincipalDataSourceException(SerializationInfo info,
                                               StreamingContext context) : base(info, context)
        {
        }

        public ExplicitlySetGenericPrincipalDataSourceException(GenericPrincipal teleoptiPrincipal)
        {
            _explicitPrincipal = teleoptiPrincipal;
        }

        protected override ITeleoptiPrincipal GetCurrentPrincipal()
        {
            if(_explicitPrincipal != null)
                return _explicitPrincipal as ITeleoptiPrincipal;
            return base.GetCurrentPrincipal();
        }
    }
}
