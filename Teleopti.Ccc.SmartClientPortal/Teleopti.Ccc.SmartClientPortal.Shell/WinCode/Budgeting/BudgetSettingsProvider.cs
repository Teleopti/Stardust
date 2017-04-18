using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Budgeting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting
{
    public interface IBudgetSettingsProvider
    {
        IBudgetSettings BudgetSettings { get; }
        void Save();
    }

    public class BudgetSettingsProvider : IBudgetSettingsProvider
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
        private IBudgetSettings _budgetSettings;

        public BudgetSettingsProvider(IUnitOfWorkFactory unitOfWorkFactory, IPersonalSettingDataRepository personalSettingDataRepository)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _personalSettingDataRepository = personalSettingDataRepository;
        }

        public IBudgetSettings BudgetSettings
        {
            get
            {
                if (_budgetSettings == null)
                {
                    using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                        _budgetSettings = _personalSettingDataRepository.FindValueByKey<IBudgetSettings>("Budget",
                                                                                                         new BudgetSettings
                                                                                                             ());
                    }
                }
                return _budgetSettings;
            }
        }

        public void Save()
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                _personalSettingDataRepository.PersistSettingValue(_budgetSettings);
                uow.PersistAll();
            }
        }
    }
}
