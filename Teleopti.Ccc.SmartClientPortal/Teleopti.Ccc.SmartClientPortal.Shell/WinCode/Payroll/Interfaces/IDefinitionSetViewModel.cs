using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll.Interfaces
{
    public interface IDefinitionSetViewModel : IViewModel<IMultiplicatorDefinitionSet>
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the type of the multiplicator.
        /// </summary>
        /// <value>The type of the multiplicator.</value>
        MultiplicatorType MultiplicatorType { get; }

        string ChangeInfo { get; }
    }
}
