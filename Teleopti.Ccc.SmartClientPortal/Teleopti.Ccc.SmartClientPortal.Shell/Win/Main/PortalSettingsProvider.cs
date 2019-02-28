using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Main;

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
                        _portalSettings = PersonalSettingDataRepository.DONT_USE_CTOR(uow).FindValueByKey("Portal", new PortalSettings());
                    }
                }
                return _portalSettings;
            }
        }
    }
}
