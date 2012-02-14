using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    public class AverageShiftLengthValueCalculator : IAverageShiftLengthValueCalculator
    {

        public double CalculateShiftValue(double shiftValue, TimeSpan workShiftWorkTime, TimeSpan averageWorkTime)
        {
            double absShiftValue = Math.Abs(shiftValue);
            double shiftPenalty = absShiftValue * deficitFromAverageFactor(shiftValue, workShiftWorkTime, averageWorkTime) - absShiftValue;
            double result = shiftValue - shiftPenalty;
            return result;
        }

        private static double deficitFromAverageFactor(double shiftValue, TimeSpan workShiftWorkTime, TimeSpan averageWorkTime)
        {
            double deficitFromAverageWorkTime = Math.Abs(workShiftWorkTime.TotalMinutes - averageWorkTime.TotalMinutes);
            double deficitPercentage = deficitFromAverageWorkTime / averageWorkTime.TotalMinutes;
            double multipicator = 1 + deficitPercentage;
            double multiplicatorFactor = calculateMultiplicatorFactor(shiftValue);
            double poweredMultipicator = Math.Pow(multipicator, multiplicatorFactor);
            return poweredMultipicator;
        }

        private static double calculateMultiplicatorFactor(double shiftValue)
        {
            if (shiftValue < 10)
                return 15d;
            return 5d;
        }
    }
}
