using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration.LegacyTransientErrorHandling
{
	[RemoveMeWithToggle(Toggles.Tech_Moving_ResilientConnectionLogic_76181)]
	public class ThrottlingCondition
	{
		private static readonly Regex sqlErrorCodeRegEx = new Regex("Code:\\s*(\\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private readonly IList<Tuple<ThrottledResourceType, ThrottlingType>> throttledResources = (IList<Tuple<ThrottledResourceType, ThrottlingType>>)new List<Tuple<ThrottledResourceType, ThrottlingType>>(9);
		public const int ThrottlingErrorNumber = 40501;

		public static ThrottlingCondition Unknown
		{
			get
			{
				ThrottlingCondition throttlingCondition = new ThrottlingCondition()
				{
					ThrottlingMode = ThrottlingMode.Unknown
				};
				throttlingCondition.throttledResources.Add(Tuple.Create<ThrottledResourceType, ThrottlingType>(ThrottledResourceType.Unknown, ThrottlingType.Unknown));
				return throttlingCondition;
			}
		}

		public ThrottlingMode ThrottlingMode { get; private set; }

		public IEnumerable<Tuple<ThrottledResourceType, ThrottlingType>> ThrottledResources
		{
			get
			{
				return (IEnumerable<Tuple<ThrottledResourceType, ThrottlingType>>)this.throttledResources;
			}
		}

		public bool IsThrottledOnDataSpace
		{
			get
			{
				return this.throttledResources.Where<Tuple<ThrottledResourceType, ThrottlingType>>((Func<Tuple<ThrottledResourceType, ThrottlingType>, bool>)(x => x.Item1 == ThrottledResourceType.PhysicalDatabaseSpace)).Any<Tuple<ThrottledResourceType, ThrottlingType>>();
			}
		}

		public bool IsThrottledOnLogSpace
		{
			get
			{
				return this.throttledResources.Where<Tuple<ThrottledResourceType, ThrottlingType>>((Func<Tuple<ThrottledResourceType, ThrottlingType>, bool>)(x => x.Item1 == ThrottledResourceType.PhysicalLogSpace)).Any<Tuple<ThrottledResourceType, ThrottlingType>>();
			}
		}

		public bool IsThrottledOnLogWrite
		{
			get
			{
				return this.throttledResources.Where<Tuple<ThrottledResourceType, ThrottlingType>>((Func<Tuple<ThrottledResourceType, ThrottlingType>, bool>)(x => x.Item1 == ThrottledResourceType.LogWriteIoDelay)).Any<Tuple<ThrottledResourceType, ThrottlingType>>();
			}
		}

		public bool IsThrottledOnDataRead
		{
			get
			{
				return this.throttledResources.Where<Tuple<ThrottledResourceType, ThrottlingType>>((Func<Tuple<ThrottledResourceType, ThrottlingType>, bool>)(x => x.Item1 == ThrottledResourceType.DataReadIoDelay)).Any<Tuple<ThrottledResourceType, ThrottlingType>>();
			}
		}

		public bool IsThrottledOnCpu
		{
			get
			{
				return this.throttledResources.Where<Tuple<ThrottledResourceType, ThrottlingType>>((Func<Tuple<ThrottledResourceType, ThrottlingType>, bool>)(x => x.Item1 == ThrottledResourceType.Cpu)).Any<Tuple<ThrottledResourceType, ThrottlingType>>();
			}
		}

		public bool IsThrottledOnDatabaseSize
		{
			get
			{
				return this.throttledResources.Where<Tuple<ThrottledResourceType, ThrottlingType>>((Func<Tuple<ThrottledResourceType, ThrottlingType>, bool>)(x => x.Item1 == ThrottledResourceType.DatabaseSize)).Any<Tuple<ThrottledResourceType, ThrottlingType>>();
			}
		}

		public bool IsThrottledOnWorkerThreads
		{
			get
			{
				return this.throttledResources.Where<Tuple<ThrottledResourceType, ThrottlingType>>((Func<Tuple<ThrottledResourceType, ThrottlingType>, bool>)(x => x.Item1 == ThrottledResourceType.WorkerThreads)).Any<Tuple<ThrottledResourceType, ThrottlingType>>();
			}
		}

		public bool IsUnknown
		{
			get
			{
				return this.ThrottlingMode == ThrottlingMode.Unknown;
			}
		}

		public static ThrottlingCondition FromException(SqlException ex)
		{
			if (ex != null)
			{
				foreach (SqlError error in ex.Errors)
				{
					if (error.Number == 40501)
						return ThrottlingCondition.FromError(error);
				}
			}
			return ThrottlingCondition.Unknown;
		}

		public static ThrottlingCondition FromError(SqlError error)
		{
			if (error != null)
			{
				Match match = ThrottlingCondition.sqlErrorCodeRegEx.Match(error.Message);
				int result;
				if (match.Success && int.TryParse(match.Groups[1].Value, out result))
					return ThrottlingCondition.FromReasonCode(result);
			}
			return ThrottlingCondition.Unknown;
		}

		public static ThrottlingCondition FromReasonCode(int reasonCode)
		{
			if (reasonCode <= 0)
				return ThrottlingCondition.Unknown;
			ThrottlingMode throttlingMode = (ThrottlingMode)(reasonCode & 3);
			ThrottlingCondition throttlingCondition = new ThrottlingCondition()
			{
				ThrottlingMode = throttlingMode
			};
			int num1 = reasonCode >> 8;
			throttlingCondition.throttledResources.Add(Tuple.Create<ThrottledResourceType, ThrottlingType>(ThrottledResourceType.PhysicalDatabaseSpace, (ThrottlingType)(num1 & 3)));
			int num2;
			throttlingCondition.throttledResources.Add(Tuple.Create<ThrottledResourceType, ThrottlingType>(ThrottledResourceType.PhysicalLogSpace, (ThrottlingType)((num2 = num1 >> 2) & 3)));
			int num3;
			throttlingCondition.throttledResources.Add(Tuple.Create<ThrottledResourceType, ThrottlingType>(ThrottledResourceType.LogWriteIoDelay, (ThrottlingType)((num3 = num2 >> 2) & 3)));
			int num4;
			throttlingCondition.throttledResources.Add(Tuple.Create<ThrottledResourceType, ThrottlingType>(ThrottledResourceType.DataReadIoDelay, (ThrottlingType)((num4 = num3 >> 2) & 3)));
			int num5;
			throttlingCondition.throttledResources.Add(Tuple.Create<ThrottledResourceType, ThrottlingType>(ThrottledResourceType.Cpu, (ThrottlingType)((num5 = num4 >> 2) & 3)));
			int num6;
			throttlingCondition.throttledResources.Add(Tuple.Create<ThrottledResourceType, ThrottlingType>(ThrottledResourceType.DatabaseSize, (ThrottlingType)((num6 = num5 >> 2) & 3)));
			int num7;
			throttlingCondition.throttledResources.Add(Tuple.Create<ThrottledResourceType, ThrottlingType>(ThrottledResourceType.Internal, (ThrottlingType)((num7 = num6 >> 2) & 3)));
			int num8;
			throttlingCondition.throttledResources.Add(Tuple.Create<ThrottledResourceType, ThrottlingType>(ThrottledResourceType.WorkerThreads, (ThrottlingType)((num8 = num7 >> 2) & 3)));
			throttlingCondition.throttledResources.Add(Tuple.Create<ThrottledResourceType, ThrottlingType>(ThrottledResourceType.Internal, (ThrottlingType)(num8 >> 2 & 3)));
			return throttlingCondition;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat((IFormatProvider)CultureInfo.CurrentCulture, "Mode: {0} | ", new object[1]
			{
				(object) this.ThrottlingMode
			});
			string[] array = this.throttledResources.Where<Tuple<ThrottledResourceType, ThrottlingType>>((Func<Tuple<ThrottledResourceType, ThrottlingType>, bool>)(x => x.Item1 != ThrottledResourceType.Internal)).Select<Tuple<ThrottledResourceType, ThrottlingType>, string>((Func<Tuple<ThrottledResourceType, ThrottlingType>, string>)(x => string.Format((IFormatProvider)CultureInfo.CurrentCulture, "{0}: {1}", new object[2]
			{
				(object) x.Item1,
				(object) x.Item2
			}))).OrderBy<string, string>((Func<string, string>)(x => x)).ToArray<string>();
			stringBuilder.Append(string.Join(", ", array));
			return stringBuilder.ToString();
		}
	}
}