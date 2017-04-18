using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.WorkloadPages
{
    /// <summary>
    /// Page manager class for workload properties
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-16
    /// </remarks>
    public class WorkloadPropertiesPages : AbstractPropertyPages<IWorkload>
    {
        private readonly ISkill _skill;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public WorkloadPropertiesPages(ISkill preselectedSkill, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory) : base(repositoryFactory,unitOfWorkFactory)
        {
            _skill = preselectedSkill;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public WorkloadPropertiesPages(IWorkload workload, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory) : base(workload,repositoryFactory,unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _skill = workload.Skill;
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
            using(AbstractWizardPages<IWorkload> ww = new WorkloadWizardPages(_skill,RepositoryFactory,_unitOfWorkFactory))
            {
                return ww.CreateNewRoot();
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
            get { return String.Empty; }
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
            get { return UserTexts.Resources.Properties; }
        }

        /// <summary>
        /// Gets the repository object.
        /// </summary>
        /// <value>The repository object.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-02-23
        /// </remarks>
        public override IRepository<IWorkload> RepositoryObject
        {
            get { return RepositoryFactory.CreateWorkloadRepository(UnitOfWork); }
        }
    }
}
