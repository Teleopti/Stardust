using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Main
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
