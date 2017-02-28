using System;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting.WorkloadPages
{
    /// <summary>
    /// Page manager class for workload wizard pages
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-16
    /// </remarks>
    public class WorkloadWizardPages : AbstractWizardPages<IWorkload>
    {
        private readonly ISkill _skill;

        public WorkloadWizardPages(ISkill preselectedSkill, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
            : base(repositoryFactory,unitOfWorkFactory)
        {
            _skill = preselectedSkill;
        }

        /// <summary>
        /// Creates the new root.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public override IWorkload CreateNewRoot()
        {
            Workload w = new Workload(_skill);
            w.Name = UserTexts.Resources.LessThanWorkloadNameGreaterThan;
            w.Description = string.Format(CultureInfo.CurrentUICulture,UserTexts.Resources.WorkloadCreatedDotParameter0, DateTime.Now);
 
            return w;
        }

        /// <summary>
        /// Afters the save.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        protected override void OnAfterSave()
        {
        }

        /// <summary>
        /// Gets the repository object.
        /// </summary>
        /// <value>The repository object.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public override IRepository<IWorkload> RepositoryObject
        {
            get
            {
                return RepositoryFactory.CreateWorkloadRepository(UnitOfWork);
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public override string Name
        {
            get { return UserTexts.Resources.WorkloadWizard; }
        }
        /// <summary>
        /// Gets the window text.
        /// </summary>
        /// <value>The window text.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-06-02
        /// </remarks>
        public override string WindowText
        {
            get { return UserTexts.Resources.NewWorkload; }
        }
    }
}