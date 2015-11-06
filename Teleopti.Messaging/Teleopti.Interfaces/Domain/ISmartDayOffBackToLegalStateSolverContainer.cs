using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Contains a matrix a solver and the bitarray
    /// </summary>
    public interface ISmartDayOffBackToLegalStateSolverContainer
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="ISmartDayOffBackToLegalStateSolverContainer"/> is result.
        /// </summary>
        /// <value><c>true</c> if result; otherwise, <c>false</c>.</value>
        bool Result { get; }

        /// <summary>
        /// Gets the matrix original state container.
        /// </summary>
        /// <value>The matrix original state container.</value>
        IScheduleMatrixOriginalStateContainer MatrixOriginalStateContainer { get; }

        /// <summary>
        /// Gets the bit array.
        /// </summary>
        /// <value>The bit array.</value>
        ILockableBitArray BitArray { get; }

        /// <summary>
        /// Gets the failed solver description keys.
        /// </summary>
        /// <value>The failed solver description keys.</value>
        IList<string> FailedSolverDescriptionKeys { get; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        void Execute(IDaysOffPreferences daysOffPreferences);
    }
}