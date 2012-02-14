using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Presentation
{

    /// <summary>
    /// Person Entity ListBox presentation class.
    /// </summary>
    public class PersonListBoxPresenter : ListBoxPresenter<IPerson>
    {
        private readonly CommonNameDescriptionSetting _commonNameDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonListBoxPresenter"/> class.
        /// </summary>
        /// <param name="domainEntity">The domain entity.</param>
        /// <param name="commonNameDescriptionSetting">The common name description setting.</param>
        public PersonListBoxPresenter(IPerson domainEntity, CommonNameDescriptionSetting commonNameDescriptionSetting) : base(domainEntity)
        {
            _commonNameDescription = commonNameDescriptionSetting;
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
            get { return _commonNameDescription.BuildCommonNameDescription(ContainedEntity); }
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
            get { return _commonNameDescription.BuildCommonNameDescription(ContainedEntity); }
        }

        #endregion
    }
}