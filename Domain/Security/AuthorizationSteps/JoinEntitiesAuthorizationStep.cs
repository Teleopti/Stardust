using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{

    /// <summary>
    /// Join strategies for entities
    /// </summary>
    public enum JoinStrategyOption
    {
        /// <summary>
        /// MergePeriods two entities
        /// </summary>
        Union,
        /// <summary>
        /// Subtract two entities
        /// </summary>
        Subtract
    }

    /// <summary>
    /// Join authorization entities step.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/27/2007
    /// </remarks>
    public class JoinEntitiesAuthorizationStep : AuthorizationStep
    {

        private JoinStrategyOption _joinStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinEntitiesAuthorizationStep"/> class.
        /// </summary>
        /// <param name="parents">The parents.</param>
        /// <param name="stepName">Name of the step.</param>
        /// <param name="joinStrategy">The join strategy.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public JoinEntitiesAuthorizationStep(
            IList<IAuthorizationStep> parents, 
            string stepName,
            JoinStrategyOption joinStrategy)
            : base(parents, stepName)
        {
            _joinStrategy = joinStrategy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinEntitiesAuthorizationStep"/> class.
        /// </summary>
        /// <param name="parents">The parents.</param>
        /// <param name="stepName">Name of the step.</param>
        /// <param name="joinStrategy">The join strategy.</param>
        /// <param name="description">The description.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public JoinEntitiesAuthorizationStep(
            IList<IAuthorizationStep> parents,
            string stepName,
            JoinStrategyOption joinStrategy, 
            string description)
            : base(parents, stepName, description)
        {
            _joinStrategy = joinStrategy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinEntitiesAuthorizationStep"/> class.
        /// </summary>
        /// <param name="firstProvider">The first provider.</param>
        /// <param name="secondProvider">The second provider.</param>
        /// <param name="panelName">Name of the panel.</param>
        /// <param name="joinStrategy">The join strategy.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public JoinEntitiesAuthorizationStep( 
            IAuthorizationStep firstProvider,
            IAuthorizationStep secondProvider,
            string panelName,
            JoinStrategyOption joinStrategy)
            : base(panelName, string.Empty)
        {
            List<IAuthorizationStep> parents = new List<IAuthorizationStep>();
            parents.Add(firstProvider);
            parents.Add(secondProvider);
            Parents = parents;
            _joinStrategy = joinStrategy;
        }

        /// <summary>
        /// Gets or sets the join strategy.
        /// </summary>
        /// <value>The join strategy.</value>
        public JoinStrategyOption JoinStrategy
        {
            get { return _joinStrategy; }
            set{ _joinStrategy = value; }
        }

        /// <summary>
        /// Refreshes the own list. Template abstact method
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2007-10-31
        /// </remarks>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        protected override IList<IAuthorizationEntity> RefreshOwnList()
        {
            bool firstItem = true;
            IList<IAuthorizationEntity> master = new List<IAuthorizationEntity>();
            foreach (IAuthorizationStep parent in Parents)
            {
                IList<IAuthorizationEntity> parentList = parent.ProvidedList<IAuthorizationEntity>();
                if (parentList != null)
                {
                    List<IAuthorizationEntity> subList = new List<IAuthorizationEntity>(parentList);
                    if (firstItem)
                    {
                        master = subList;
                        firstItem = false;
                    }
                    else
                    {
                        if (JoinStrategy == JoinStrategyOption.Union)
                            master = AuthorizationEntityExtender.UnionTwoLists(master, subList);
                        else
                            master = AuthorizationEntityExtender.SubtractTwoLists(master, subList);
                    }
                }
            }
            return master;
        }

    }
}
