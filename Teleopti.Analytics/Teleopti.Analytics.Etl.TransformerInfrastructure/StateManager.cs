using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    /// <summary>
    /// Stores application data for the application lifetime  
    /// </summary>
    internal class StateManager : State
    {

		#region Fields (1)

        private ISessionData _sessData;

		#endregion Fields

		#region Properties (1)

        /// <summary>
        /// Gets the session data for logged on personItem.
        /// </summary>
        /// <value>The session data.</value>
        public override ISessionData SessionScopeData
        {
            get { return _sessData; }
        }

		#endregion Properties

		#region Methods (2)


		// Public Methods (2)

        /// <summary>
        /// Clears data kept for logged in session/personItem
        /// </summary>
        public override void ClearSession()
        {
            _sessData = null;
        }

        /// <summary>
        /// Sets the session data.
        /// </summary>
        /// <param name="sessionData">The sess data.</param>
        public override void SetSessionData(ISessionData sessionData)
        {
            _sessData = sessionData;
        }

		#endregion Methods
    }
}