using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.SkillPages
{
    /// <summary>
    /// Page manager class for skill properties
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-16
    /// </remarks>
    public class SkillPropertiesPages : AbstractPropertyPages<ISkill>
    {
        private readonly ISkillType _preselectedSkillType;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        /// Creates the new root.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public override ISkill CreateNewRoot()
        {
            using(AbstractWizardPages<ISkill> wp = new SkillWizardPages(_preselectedSkillType,RepositoryFactory,_unitOfWorkFactory))
            {
                return wp.CreateNewRoot();
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
        public override IRepository<ISkill> RepositoryObject
        {
            get { return RepositoryFactory.CreateSkillRepository(UnitOfWork); }
        }

        public SkillPropertiesPages(ISkillType preselectedSkillType, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory) : base(repositoryFactory,unitOfWorkFactory)
        {
            _preselectedSkillType = preselectedSkillType;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public SkillPropertiesPages(ISkill preselectedSkill, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
            : base(preselectedSkill,repositoryFactory,unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }
    }
}
