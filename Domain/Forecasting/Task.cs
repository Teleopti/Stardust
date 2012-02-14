using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// Holds information about a person's task including time length information.
    /// </summary>
    public class Task : ITask
    {
        private double _tasks;
        private TimeSpan _averageTaskTime;
        private TimeSpan _averageAfterTaskTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> struct.
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 19.12.2007
        /// </remarks>
        public Task()
        {
            _averageTaskTime = TimeSpan.Zero;
            _averageAfterTaskTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class.
        /// </summary>
        /// <param name="tasks">The tasks.</param>
        /// <param name="averageTaskTime">The average task time.</param>
        /// <param name="averageAfterTaskTime">The average after task time.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 21.12.2007
        /// </remarks>
        public Task(double tasks, TimeSpan averageTaskTime, TimeSpan averageAfterTaskTime)
        {
            //InParameter.CheckTimeSpanAtLeastOneTick("averageTaskTime", averageTaskTime);
            //InParameter.CheckTimeSpanAtLeastOneTick("averageAfterTaskTime", averageAfterTaskTime);
            _tasks = tasks;
            _averageTaskTime = averageTaskTime;
            _averageAfterTaskTime = averageAfterTaskTime;
        }

        #region Tasks
        /// <summary>
        /// Gets the tasks.
        /// </summary>
        /// <value>The tasks.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-15
        /// </remarks>
        public double Tasks
        {
            get { return _tasks; }
        }

        #endregion

        #region Time



        /// <summary>
        /// Gets the average task time.
        /// </summary>
        /// <value>The average task time.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 21.12.2007
        /// </remarks>
        public TimeSpan AverageTaskTime
        {
            get { return _averageTaskTime; }
        }

        #endregion

        #region AfterCallWorkTime


        /// <summary>
        /// Gets the average after task time.
        /// </summary>
        /// <value>The average after task time.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 21.12.2007
        /// </remarks>
        public TimeSpan AverageAfterTaskTime
        {
            get { return _averageAfterTaskTime; }
        }
        
        #endregion

        #region IEquatable<Task> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public bool Equals(ITask other)
        {
            if (other == null)
                return false;

            return other.AverageTaskTime == _averageTaskTime &&
                   other.AverageAfterTaskTime == _averageAfterTaskTime &&
                   other.Tasks == _tasks;

        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Task))
            {
                return false;
            }
            else
            {
                return Equals((Task)obj);
            }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public override int GetHashCode()
        {
            return (GetType().FullName + "|" + 
                _averageTaskTime + "|" + 
                _averageAfterTaskTime + "|" + 
                _tasks).GetHashCode();
                //_averageTaskTime.GetHashCode() ^
                  // _averageAfterTaskTime.GetHashCode() ^
                  // _tasks.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="task1">The task1.</param>
        /// <param name="task2">The task2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public static bool operator ==(Task task1, Task task2)
        {
            if ((object)task1 == null)
                return false;

            if ((object)task2 == null)
                return false;

            return task1.Equals(task2);
        }


        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="task1">The task1.</param>
        /// <param name="task2">The task2.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-16
        /// </remarks>
        public static bool operator !=(Task task1, Task task2)
        {
            if ((object)task1 == null && (object)task2 == null)
                return false;

            if ((object)task1 == null)
                return true;

            if ((object)task2 == null)
                return true;

            return !task1.Equals(task2);
        }

        #endregion
    }
}