using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting;
using Teleopti.Ccc.WinCode.Budgeting;

namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [TestFixture]
    public class BudgetSettingsProviderTest
    {
        private IBudgetSettingsProvider target;
        private MockRepository mocks;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private IPersonalSettingDataRepository personalSettingsRepository;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            personalSettingsRepository = mocks.StrictMock<IPersonalSettingDataRepository>();
            target = new BudgetSettingsProvider(unitOfWorkFactory,personalSettingsRepository);
        }

        [Test]
        public void ShouldLoadSettings()
        {
            IBudgetSettings budgetSettings = mocks.DynamicMock<IBudgetSettings>();
            IUnitOfWork unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            using (mocks.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(personalSettingsRepository.FindValueByKey<IBudgetSettings>("Budget", new BudgetSettings())).Return(budgetSettings).IgnoreArguments();
            }
            using (mocks.Playback())
            {
                target.BudgetSettings.Should().Be.EqualTo(budgetSettings);
            }
        }

        [Test]
        public void ShouldSaveSettings()
        {
            IUnitOfWork unitOfWork = mocks.DynamicMock<IUnitOfWork>();
            using (mocks.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(personalSettingsRepository.PersistSettingValue(null)).IgnoreArguments().Return(null);
                Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
            }
            using (mocks.Playback())
            {
                target.Save();
            }
        }
    }
}
