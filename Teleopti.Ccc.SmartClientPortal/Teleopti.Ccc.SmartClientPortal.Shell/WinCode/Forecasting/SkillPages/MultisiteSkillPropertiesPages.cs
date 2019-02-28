using System;
using System.Drawing;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.SkillPages
{
    /// <summary>
    /// Page manager class for multisite skill properties
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-16
    /// </remarks>
    public class MultisiteSkillPropertiesPages : AbstractPropertyPages<IMultisiteSkill>
    {
        private readonly ISkillType _preselectedSkillType;

        /// <summary>
        /// Creates the new root.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public override IMultisiteSkill CreateNewRoot()
        {
            return new MultisiteSkill(".",".",Color.FromArgb(0),15,_preselectedSkillType);
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
        public override IRepository<IMultisiteSkill> RepositoryObject
        {
            get { return MultisiteSkillRepository.DONT_USE_CTOR(UnitOfWork); }
        }

        public MultisiteSkillPropertiesPages(ISkillType preselectedSkillType, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
            : base(repositoryFactory,unitOfWorkFactory)
        {
            _preselectedSkillType = preselectedSkillType;
        }

        public MultisiteSkillPropertiesPages(IMultisiteSkill preselectedSkill, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
            : base(preselectedSkill,repositoryFactory,unitOfWorkFactory)
        {
        }
    }
}
