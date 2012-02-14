
namespace Teleopti.Ccc.WinCode.Payroll.Interfaces
{
    public interface IViewModel<T>
    {
        /// <summary>
        /// Gets the domain entity.
        /// </summary>
        /// <value>The domain entity.</value>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        T DomainEntity { get; }
    }
}
