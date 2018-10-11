using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Exceptions;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IAuditableBpoOperations
	{
		ClearBpoReturnObject ClearBpoForPeriod(ClearBpoActionObj clearBpoActionObj);
		ImportBpoFileResult ImportBpo(ImportBpoActionObj fileContents);
	}

	public class AuditableBpoOperationsToggleOn : IAuditableBpoOperations
	{
		private readonly BpoProvider _bpoProvider;
		private readonly ImportBpoFile _bpoFile;

		public AuditableBpoOperationsToggleOn(BpoProvider bpoProvider, ImportBpoFile bpoFile)
		{
			_bpoProvider = bpoProvider;
			_bpoFile = bpoFile;
		}

		[AuditTrail]
		public virtual ClearBpoReturnObject ClearBpoForPeriod(ClearBpoActionObj clearBpoActionObj)
		{
			var result = _bpoProvider.ClearBpoResources(clearBpoActionObj.BpoGuid, clearBpoActionObj.StartDate, clearBpoActionObj.EndDate.AddDays(1).AddMinutes(-1));
			if (String.IsNullOrEmpty(result.ErrorMessage))
				return result;
			throw new BusinessRuleBrokenException() { ReturnObject = result };

		}

		[AuditTrail]
		public virtual ImportBpoFileResult ImportBpo(ImportBpoActionObj fileContents)
		{
			var result = _bpoFile.ImportFile(fileContents.FileContent, CultureInfo.InvariantCulture, fileContents.FileName);
			if (result.Success)
				return result;
			throw new BusinessRuleBrokenException(){ReturnObject = result};
		}
	}
	

	public class AuditableBpoOperationsToggleOff : IAuditableBpoOperations
	{
		private readonly BpoProvider _bpoProvider;
		private readonly ImportBpoFile _bpoFile;

		public AuditableBpoOperationsToggleOff(BpoProvider bpoProvider, ImportBpoFile bpoFile)
		{
			_bpoProvider = bpoProvider;
			_bpoFile = bpoFile;
		}

		public virtual ClearBpoReturnObject ClearBpoForPeriod(ClearBpoActionObj clearBpoActionObj)
		{
			return _bpoProvider.ClearBpoResources(clearBpoActionObj.BpoGuid, clearBpoActionObj.StartDate, clearBpoActionObj.EndDate.AddDays(1).AddMinutes(-1));
		}

		public virtual ImportBpoFileResult ImportBpo(ImportBpoActionObj fileContents)
		{
			var result = _bpoFile.ImportFile(fileContents.FileContent, CultureInfo.InvariantCulture, fileContents.FileName);
			return result;
		}
	}
}