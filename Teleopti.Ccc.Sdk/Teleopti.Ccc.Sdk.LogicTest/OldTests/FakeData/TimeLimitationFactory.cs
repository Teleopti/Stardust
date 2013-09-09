using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests.FakeData
{
    public static class TimeLimitationFactory
    {

        /// <summary>
        /// Creates the start time limitation.
        /// </summary>
        /// <param name="startHour">The start hour.</param>
        /// <param name="startMin">The start min.</param>
        /// <param name="startSec">The start sec.</param>
        /// <param name="endHour">The end hour.</param>
        /// <param name="endMin">The end min.</param>
        /// <param name="endSec">The end sec.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-11-03
        /// </remarks>
        public static StartTimeLimitation CreateStartTimeLimitation(int startHour, int startMin, int startSec,
                                                                    int endHour, int endMin, int endSec)
        {
            TimeSpan? start = new TimeSpan(startHour, startMin, startSec);
            TimeSpan? end = new TimeSpan(endHour, endMin, endSec);
            return new StartTimeLimitation(start, end);
        }

        /// <summary>
        /// Creates the end time limitation.
        /// </summary>
        /// <param name="startHour">The start hour.</param>
        /// <param name="startMin">The start min.</param>
        /// <param name="startSec">The start sec.</param>
        /// <param name="endHour">The end hour.</param>
        /// <param name="endMin">The end min.</param>
        /// <param name="endSec">The end sec.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-11-03
        /// </remarks>
        public static EndTimeLimitation CreateEndTimeLimitation(int startHour, int startMin, int startSec,
                                                                int endHour, int endMin, int endSec)
        {
            TimeSpan? start = new TimeSpan(startHour, startMin, startSec);
            TimeSpan? end = new TimeSpan(endHour, endMin, endSec);
            return new EndTimeLimitation(start, end);
        }


        /// <summary>
        /// Creates the work time limitation.
        /// </summary>
        /// <param name="startHour">The start hour.</param>
        /// <param name="startMinutes">The start minutes.</param>
        /// <param name="endHour">The end hour.</param>
        /// <param name="endMinutes">The end minutes.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-11-11
        /// </remarks>
        public static WorkTimeLimitation CreateWorkTimeLimitation(int startHour, int startMinutes, int endHour, int endMinutes) 
        {
            TimeSpan? start = new TimeSpan(startHour, startMinutes,0);
            TimeSpan? end = new TimeSpan(endHour, endMinutes,0);
            return new WorkTimeLimitation(start, end);
        }
    }
}