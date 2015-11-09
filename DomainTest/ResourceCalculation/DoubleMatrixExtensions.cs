using System.Diagnostics;
using System.Globalization;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	public static class DoubleMatrixExtensions
	{
		public static void DebugWriteMatrix(this double[,] a)
		{
			for (int iAgent = 0; iAgent <= a.GetUpperBound(0); iAgent++)
			{
				for (int jSkill = 0; jSkill <= a.GetUpperBound(1); jSkill++)
				{
					Debug.Write(a[iAgent, jSkill].ToString("0.0000", CultureInfo.CurrentCulture) + "\t");
				}
				Debug.WriteLine("");
			}
			Debug.WriteLine("");
		}
	}
}