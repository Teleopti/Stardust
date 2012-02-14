using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// Data for logged on User
    /// </summary>
    public class SessionData : ISessionData
    {
        private ICccTimeZoneInfo _timeZone;
        
        public SessionData()
        {
            _timeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
        }

        public ICccTimeZoneInfo TimeZone
        {
            get { return _timeZone; }
            set
            {
                InParameter.NotNull("TimeZone", value);
                _timeZone = value;
            }
        }

        public bool MickeMode { get; set; }

        public object Clip { get; set; }

        public AuthenticationTypeOption AuthenticationTypeOption { get; set; }
    }
}
