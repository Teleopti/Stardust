using System;

namespace Teleopti.Ccc.Domain.Forecasting.ErlangA
{
	public static class MathFormulas
	{

		public static Tuple<double, double> Factorial(double i)
		{
			double factorial = 1;
			double devidedBy = 0;

			for (int j = 2; j <= i; j++)
			{
				while (double.IsPositiveInfinity(factorial * j))
				{
					devidedBy++;
					factorial = factorial / 10000;
				}
				factorial *= j;
			}
			return new Tuple<double, double>(factorial, devidedBy);
		}

		public static Tuple<double, double> PowerOf(double x, int y)
		{
			double product = 1;
			double devidedBy = 0;

			for (int i = 1; i <= y; i++)
			{
				while (double.IsPositiveInfinity(product * x))
				{
					product = product / 10000;
					devidedBy++;
				}
				product *= x;
			}
			return new Tuple<double, double>(product, devidedBy);
		}

		public static Tuple<double, double> DevidedBy(Tuple<double, double> products, double y)
		{
			double product = products.Item1;
			double devidedBy = products.Item2;

			while (!double.IsPositiveInfinity(product * 10000))
			{
				product *= 10000;
				devidedBy--;
			}

			while (double.Epsilon > (product / y))
			{
				product = product * 10000;
				devidedBy--;
			}
			product /= y;

			return new Tuple<double, double>(product, devidedBy);
		}

		public static Tuple<double, double> ItterativPowerOf(Tuple<double, double> products, double y)
		{
			double product = products.Item1;
			double devidedBy = products.Item2;

			while (double.IsPositiveInfinity(product * y))
			{
				product = product / 10000;
				devidedBy++;
			}
			product *= y;

			return new Tuple<double, double>(product, devidedBy);
		}

		public static Tuple<double, double> Multiply(Tuple<double, double> products, double y)
		{
			if (y < 2)
			{
				return products;
			}

			double product = products.Item1;
			double devidedBy = products.Item2;

			while (double.IsPositiveInfinity(product * y))
			{
				product = product / 10000;
				devidedBy++;
			}
			product *= y;

			return new Tuple<double, double>(product, devidedBy);
		}
	}
}