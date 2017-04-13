using Teleopti.Ccc.WinCode.Payroll.Interfaces;

namespace Teleopti.Ccc.WinCode.Payroll
{
    public class ViewModel<T> : IViewModel<T>
    {
        private readonly T _domainEntity;

        /// <summary>
        /// Gets the domain entity.
        /// </summary>
        /// <value>The domain entity.</value>
        public T DomainEntity
        {
            get { return _domainEntity; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="domainEntity">The domain entity.</param>
        protected ViewModel(T domainEntity)
        {
            _domainEntity = domainEntity;
        }
    }
}
