using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Main
{
    public class PortalSettingsProvider
    {
        private PortalSettings _portalSettings;

        public PortalSettings PortalSettings
        {
            get
            {
                if (_portalSettings == null)
                {
                    using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                    {
                        _portalSettings = new PersonalSettingDataRepository(uow).FindValueByKey("Portal", new PortalSettings());
                    }
                }
                return _portalSettings;
            }
        }
    }
}
