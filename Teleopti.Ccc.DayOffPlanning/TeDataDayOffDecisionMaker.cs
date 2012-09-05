using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Te")]
	public class TeDataDayOffDecisionMaker : IDayOffDecisionMaker
	{

		//if not 2-2-2-2 return false
		//find the best two connecting days in same week to move from
		//find the best two connecting days in same week as above to move to, do not validate max cons work days
		//execute the move in the bittarray
		//validate max cons work days, if ok return the moves

		//goto week before
		//find the best two connecting days in that week to move from
		//find the best two connecting days in that week as above to move to, do not validate max cons work days
		//execute the move in the bittarray
		//validate max cons work days, if ok return the moves

		//repeate until success or no more weeks


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private readonly IList<IDayOffLegalStateValidator> _validatorListWithoutMaxConsecutiveWorkdays;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private readonly IDayOffLegalStateValidator _maxConsecutiveWorkdaysValidator;
		private readonly bool _is2222;
		private readonly ILogWriter _logWriter;

		public TeDataDayOffDecisionMaker(
            IList<IDayOffLegalStateValidator> validatorListWithoutMaxConsecutiveWorkdays, 
			IDayOffLegalStateValidator maxConsecutiveWorkdaysValidator,
			bool is2222,
            ILogWriter logWriter)
		{
			_validatorListWithoutMaxConsecutiveWorkdays = validatorListWithoutMaxConsecutiveWorkdays;
			_maxConsecutiveWorkdaysValidator = maxConsecutiveWorkdaysValidator;
			_is2222 = is2222;
			_logWriter = logWriter;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
		public bool Execute(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			if (!_is2222)
				return false;

			string decisionMakerName = this.ToString();
			_logWriter.LogInfo("Execute of " + decisionMakerName);

			IEnumerable<int> indexesToMoveFrom = createPreferredIndexesToMoveFrom(lockableBitArray, values);
			IEnumerable<int> indexesToMoveTo = createPreferredIndexesToMoveTo(lockableBitArray, values);

			_logWriter.LogInfo("Move from preference index: " + createCommaSeparatedString(indexesToMoveFrom));
			_logWriter.LogInfo("Move to preference index: " + createCommaSeparatedString(indexesToMoveTo));

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "values"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "lockableBitArray")]
		private static IEnumerable<int> createPreferredIndexesToMoveFrom(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			//List<KeyValuePair<int, double>> test = new List<KeyValuePair<int, double>>();

			IList<int> ret = new List<int>();

			

			return ret;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "values"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "lockableBitArray")]
		private static IEnumerable<int> createPreferredIndexesToMoveTo(ILockableBitArray lockableBitArray, IList<double?> values)
		{
			//List<KeyValuePair<int, double>> test = new List<KeyValuePair<int, double>>();

			IList<int> ret = new List<int>();

			

			return ret;
		}

		private static string createCommaSeparatedString(IEnumerable<int> indexesToMoveFrom)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (int i in indexesToMoveFrom)
			{
				stringBuilder.Append(i.ToString(CultureInfo.CurrentCulture) + ",");
			}
			string result = stringBuilder.ToString();
			if (result.Length > 0)
				result = result.Substring(0, result.Length - 1);
			return result;
		}
	}
}