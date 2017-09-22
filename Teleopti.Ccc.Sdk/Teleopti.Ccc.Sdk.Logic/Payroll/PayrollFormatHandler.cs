using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Payroll
{
    public class PayrollFormatHandler
    {
	    private readonly IPayrollFormatRepository _payrollFormatRepository;
	    private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

	    public PayrollFormatHandler(IPayrollFormatRepository payrollFormatRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
	    {
		    _payrollFormatRepository = payrollFormatRepository;
		    _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
	    }

	    public void Save(ICollection<PayrollFormatDto> payrollFormatDtos)
        {
            if (payrollFormatDtos == null) return;

			using (var unitOfWork = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var oldOnes = _payrollFormatRepository.LoadAll();
				foreach (var payrollFormat in oldOnes)
				{
					_payrollFormatRepository.Remove(payrollFormat);
				}

				foreach (var payrollFormatDto in payrollFormatDtos)
				{
					var format = new PayrollFormat { Name = payrollFormatDto.Name, FormatId = payrollFormatDto.FormatId };
					_payrollFormatRepository.Add(format);
				}
				unitOfWork.PersistAll();
			}
        }

        public ICollection<PayrollFormatDto> Load(string dataSource)
        {
	        using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
	        {
				var formats = _payrollFormatRepository.LoadAll();
		        return formats.Select(f => new PayrollFormatDto(f.FormatId, f.Name, dataSource)).ToArray();
	        }
        }
    }
}