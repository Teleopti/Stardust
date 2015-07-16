using System;
using System.Globalization;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.Win.Sikuli.Helpers;

namespace Teleopti.Ccc.Win.Sikuli.Validators.AtomicValidators
{
	internal class MemoryUsageValidator : IAtomicValidator
	{
		private readonly double _memoryUsageLimit;
		private readonly MemoryCounter _memoryCounter;

		public MemoryUsageValidator(Double memoryUsageLimit, MemoryCounter memoryCounter)
		{
			_memoryUsageLimit = memoryUsageLimit;
			_memoryCounter = memoryCounter;
		}

		public string Description
		{
			get { return "Memory usage must be under limit."; }
		}

		public SikuliValidationResult Validate()
		{
			var result = new SikuliValidationResult();

			if (_memoryCounter.CurrentMemoryConsumption() > _memoryUsageLimit)
				result.Result = SikuliValidationResult.ResultValue.Warn;

			result.AppendResultLine("Memory:", 
				string.Format(CultureInfo.CurrentCulture, "{0:#} MB", _memoryUsageLimit), 
				string.Format(CultureInfo.CurrentCulture, "{0:#} MB (max: {1:#} MB)", _memoryCounter.CurrentMemoryConsumption(), _memoryCounter.MaximumMemoryConsumption), result.Result);
			
			return result;
		}
	}
}
