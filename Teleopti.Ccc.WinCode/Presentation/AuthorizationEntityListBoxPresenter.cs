using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Presentation
{
    /// <summary>
    /// AuthorizationEntity presentation helper class.
    /// </summary>
    public class AuthorizationEntityListBoxPresenter : 
        ListBoxPresenter<IAuthorizationEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationEntityListBoxPresenter"/> class.
        /// </summary>
        /// <param name="domainEntity">The domain entity.</param>
        public AuthorizationEntityListBoxPresenter(IAuthorizationEntity domainEntity)
            : base(domainEntity)
        {
            //
        }

        /// <summary>
        /// Gets the databind text.
        /// </summary>
        /// <value>The databind text.</value>
        public override string DataBindText
        {
            get { return ContainedEntity.AuthorizationName; }
        }

        /// <summary>
        /// Gets the databind description text.
        /// </summary>
        /// <value>The databind description text.</value>
        public override string DataBindDescriptionText
        {
            get { return ContainedEntity.AuthorizationDescription; }
        }
    }
}