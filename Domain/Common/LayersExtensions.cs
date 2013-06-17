using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class LayersExtensions
	{
		 public static DateTimePeriod? Period(this IEnumerable<ILayer<IPayload>> layers)
		 {
			 DateTimePeriod? ret = null;
			 foreach (var layer in layers)
			 {
				 if (layer != null)
				 {
					 ret = DateTimePeriod.MaximumPeriod(ret, layer.Period);
				 }
			 }
			 return ret;
		 }
	}
}