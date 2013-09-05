using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Presentation
{

    /// <summary>
    /// BusinessUnit Entity ListBox presentation class.
    /// </summary>
    public class BusinessUnitListBoxPresenter : ListBoxPresenter<IBusinessUnit>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessUnitListBoxPresenter"/> class.
        /// </summary>
        /// <param name="domainEntity">The domain entity.</param>
        public BusinessUnitListBoxPresenter(IBusinessUnit domainEntity) : base(domainEntity)
        {
            //
        }

        #region ListBoxPresenter abstract methods implementation

        /// <summary>
        /// Gets the Name value. Usually this is the short name.
        /// </summary>
        /// <value>The key field.</value>
        /// <remarks>
        /// Usually this value goes to the name field of a control.
        /// </remarks>
        public override string DataBindText
        {
            get { return ContainedEntity.Description.ShortName; }
        }


        /// <summary>
        /// Gets the description or additional info value. Usually that is
        /// a longer description about the entity.
        /// </summary>
        /// <value>The description field.</value>
        /// <remarks>
        /// Usually this value goes to the tooltip to the control.
        /// </remarks>
        public override string DataBindDescriptionText
        {
            get { return ContainedEntity.Description.Name; }
        }

        #endregion
    }
}