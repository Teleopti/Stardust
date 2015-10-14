using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Payroll;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.Payroll
{
    [TestFixture]
    public class PayrollFormatHandlerTest
    {
        private PayrollFormatHandler _formatHandler;

        [SetUp]
        public void Setup()
        {
			_formatHandler = new PayrollFormatHandler(new FakePayrollFormatRepository(), new FakeCurrentUnitOfWorkFactory());
        }

        [Test]
        public void ShouldReadPayrollFormatsFromStorage()
        {
            var payrollFormatDtos = new List<PayrollFormatDto> { new PayrollFormatDto(new Guid("1E88583F-284C-4C17-BD07-B7AFA08D0BF4"), "Sweet Päjrållformat", "tjo") };
            //First Save a file
            _formatHandler.Save(payrollFormatDtos);

            var formats =  _formatHandler.Load("tjo");
            Assert.AreEqual("Sweet Päjrållformat", formats.First().Name);
            Assert.AreEqual(new Guid("1E88583F-284C-4C17-BD07-B7AFA08D0BF4"), formats.First().FormatId);
        }

	    private class FakePayrollFormatRepository : IPayrollFormatRepository
	    {
		    private readonly IList<IPayrollFormat> internalStore = new List<IPayrollFormat>();

		    public void Add(IPayrollFormat root)
		    {
			    internalStore.Add(root);
		    }

		    public void Remove(IPayrollFormat root)
		    {
			    throw new NotImplementedException();
		    }

		    public IPayrollFormat Get(Guid id)
		    {
			    throw new NotImplementedException();
		    }

		    public IList<IPayrollFormat> LoadAll()
		    {
			    return internalStore;
		    }

		    public IPayrollFormat Load(Guid id)
		    {
			    throw new NotImplementedException();
		    }

		    public long CountAllEntities()
		    {
			    throw new NotImplementedException();
		    }

		    public void AddRange(IEnumerable<IPayrollFormat> entityCollection)
		    {
			    throw new NotImplementedException();
		    }

		    public IUnitOfWork UnitOfWork { get; private set; }
	    }
    }
}
