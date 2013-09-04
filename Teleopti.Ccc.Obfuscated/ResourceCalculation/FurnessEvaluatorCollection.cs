using System;
using System.Collections.Generic;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    /// <summary>
    /// Fratar optimizer representer.
    /// </summary>
    public class FurnessEvaluatorCollection<T> : List<T> where T : FurnessEvaluator
    {

        #region Variables

        private double _quotient = 1d;

        /// <summary>
        /// The number of the actual outer iteration.
        /// </summary>
        private int _outerIteration;

        #endregion

        #region Interface

        /// <summary>
        /// Overall evaluation on all the members.
        /// </summary>
        /// <param name="maxOuterIteration">The max number of outer iteration.</param>
        /// <param name="maxInnerIteration">The max number of inner iteration.</param>
        /// <returns>
        /// The quotient, calculated by during the optimization process
        /// </returns>
        public double Evaluate(int maxOuterIteration, int maxInnerIteration)
        {
            int iteration = 0;

            do
            {
                iteration++;
                EvaluateMembers(Quotient, maxInnerIteration);
            } while (!IsStabilized(1d/10000d) && iteration < maxOuterIteration);

            _outerIteration = iteration;

            return Quotient;
        }

        /// <summary>
        /// Overall evaluation on all the members.
        /// </summary>
        /// <returns>
        /// The quotient, calculated by during the optimization process
        /// </returns>
        public double Evaluate()
        {
            return Evaluate(8, 8);
        }

        #endregion

        #region Local methods

        /// <summary>
        /// Evaluates all the <see cref="FurnessEvaluator"/> members.
        /// </summary>
        /// <returns>Total values.</returns>
        protected double EvaluateMembers(double quotient, int maxIteration)
        {
            double ret = 0d;
            foreach (T item in this)
            {
                ret += item.Evaluate(quotient, maxIteration);
            }
            return ret;
        }

        /// <summary>
        /// Gets or sets the current quotient.
        /// </summary>
        /// <value>The current quotient.</value>
        protected double Quotient
        {
            get { return _quotient; }
            set { _quotient = value; }
        }

        /// <summary>
        /// The number of the actual outer iteration.
        /// </summary>
        public int OuterIteration
        {
            get { return _outerIteration; }
        }

        /// <summary>
        /// Calculates the current quotient.
        /// </summary>
        protected double CalculateQuotient()
        {
            return TotalProduction() / TotalProductionDemand();
        }

        /// <summary>
        /// Gets the total production demand.
        /// </summary>
        protected double TotalProductionDemand()
        {
            double ret = 0d;
            foreach (T item in this)
            {
                ret += item.Data.TotalProductionDemand();
            }
            return ret;
        }

        /// <summary>
        /// Gets the total production.
        /// </summary>
        protected double TotalProduction()
        {
            double ret = 0d;
            foreach (T item in this)
            {
                ret += item.Data.TotalProduction();
            }
            return ret;
        }

        /// <summary>
        /// Determines whether the iteration is stabilized.
        /// </summary>
        /// <param name="expectedStabilizationFactor">The expected stabilization factor.</param>
        /// <returns>
        /// 	<c>true</c> if the iteration is stabilized; otherwise, <c>false</c>.
        /// </returns>
        private bool IsStabilized(double expectedStabilizationFactor)
        {
            double newQuotient = CalculateQuotient();
            {
                double stabilizationFactor = (Math.Abs(Quotient - newQuotient)) / (Quotient + newQuotient);
                if (stabilizationFactor < expectedStabilizationFactor)
                {
                    Quotient = newQuotient;
                    return true;
                }
                else
                {
                    Quotient = newQuotient;
                    return false;
                }
            }
        }

        #endregion

    }
}