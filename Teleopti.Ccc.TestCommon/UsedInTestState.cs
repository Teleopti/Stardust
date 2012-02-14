using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
    public class UsedInTestState : State
    {
        private ISessionData _sessData;

        /// <summary>
        /// Sets the session data.
        /// </summary>
        /// <param name="sessData">The sess data.</param>
        public override void SetSessionData(ISessionData sessData)
        {
            _sessData = sessData;
        }

        /// <summary>
        /// Gets the session data for logged on user.
        /// </summary>
        /// <value>The session data.</value>
        public override ISessionData SessionScopeData
        {
            get { return _sessData; }
        }

        /// <summary>
        /// Clears data kept for logged in session/user
        /// </summary>
        public override void ClearSession()
        {
            _sessData = null;
        }
    }
}