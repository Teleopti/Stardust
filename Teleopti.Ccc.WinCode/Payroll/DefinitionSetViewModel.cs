using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll
{
    public class DefinitionSetViewModel : ViewModel<IMultiplicatorDefinitionSet>, IDefinitionSetViewModel
    {
        private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();
        #region Properties -Instance Members

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return DomainEntity.Name;
            }
        }

        /// <summary>
        /// Gets the type of the multiplicator.
        /// </summary>
        /// <value>The type of the multiplicator.</value>
        public MultiplicatorType MultiplicatorType
        {
            get
            {
                return DomainEntity.MultiplicatorType;
            }
        }

        public string ChangeInfo
        {
            get
            {
                return _localizer.UpdatedByText(DomainEntity, UserTexts.Resources.UpdatedByColon);
            }
        }

        #endregion

        #region Methods - Instance Members

        #region Methods - Instance Members - Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionSetViewModel"/> class.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        public DefinitionSetViewModel(IMultiplicatorDefinitionSet definitionSet)
            : base(definitionSet)
        {

        }

        #endregion

        #endregion
    }
}
