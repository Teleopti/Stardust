using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    /// <summary>
    /// Tests the AssignmentLength restriction
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-07
    /// </remarks>
    public abstract class RestrictionTest<T> where T : IAggregateRoot
    {
        private IRestriction<T> _target;

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>The target.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        protected IRestriction<T> Target
        {
            get { return _target; }
            set { _target = value; }
        }

        [SetUp]
        public void Setup()
        {
            ConcreteSetup();
        }

        #region Abstract methods

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected abstract void ConcreteSetup();

        /// <summary>
        /// Creates the invalid entity to verify.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        protected abstract T CreateInvalidEntityToVerify();

        /// <summary>
        /// Creates the valid entity to verify.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        protected abstract T CreateValidEntityToVerify();

        #endregion

        /// <summary>
        /// Verifies the check entity works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void VerifyCheckEntityGivesValidationException()
        {
            Target.CheckEntity(CreateInvalidEntityToVerify());
        }

        /// <summary>
        /// Verifies valid entity in check entity gives no exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        [Test]
        public void VerifyValidCheckEntityGivesNoException()
        {
            T validEntity = CreateValidEntityToVerify();
            Target.CheckEntity(validEntity);

            Assert.IsNotNull(validEntity);
        }
    }
}