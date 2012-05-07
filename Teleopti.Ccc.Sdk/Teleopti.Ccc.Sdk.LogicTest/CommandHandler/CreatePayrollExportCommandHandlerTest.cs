using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class CreatePayrollExportCommandHandlerTest
    {
        private IPayrollResultFactory _payrollResultFactory;
        private MockRepository _mock;
        private CreatePayrollExportCommandHandler _target;

        [SetUp]
        public void Setup()
        {
            _mock  = new MockRepository();
            _payrollResultFactory = _mock.StrictMock<IPayrollResultFactory>();
            _target = new CreatePayrollExportCommandHandler(_payrollResultFactory);
        }

        [Test]
        public void ShouldProcessCreatePayrollExportCommand()
        {
            var payrollExportDto = new PayrollExportDto();
            var createPayrollExportCommandDto = new CreatePayrollExportCommandDto();
            createPayrollExportCommandDto.PayrollExportDto = payrollExportDto;

            using(_mock.Record())
            {
                Expect.Call(_payrollResultFactory.RunPayrollOnBus(createPayrollExportCommandDto.PayrollExportDto)).
                    IgnoreArguments().Return(Guid.NewGuid());
            }
            using(_mock.Playback())
            {
                _target.Handle(createPayrollExportCommandDto);
            }

        }
    }
}
