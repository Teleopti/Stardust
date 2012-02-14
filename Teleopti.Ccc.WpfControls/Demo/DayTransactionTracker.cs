using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WpfControls.Demo
{
    /// <summary>
    ///TestTracker ´för att läsa "live data"
    /// Kastas!! Fulkod!
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-08-27
    /// </remarks>
    public class DayTransactionTracker:ITracker<int>,IScheduleExtractor
    {
       IScheduleRange _scheduleRange;
            private Description _trackingDescription;
            private IList<DateTime> _trackedLayers = new List<DateTime>();
            private ISchedulePart _schedulePart;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dateTimePeriodToCheck")]
        public int Used(DateTimePeriod dateTimePeriodToCheck)
            {
                //Does not care about the date, it will track everything in the range with a projection
                _trackedLayers.Clear();

                if (_schedulePart == null) return 0;

                var trackedItems =
                    (from l in _schedulePart.ProjectionService().CreateProjection()
                     select l.Period.StartDateTime.Date).Distinct();

                foreach (DateTime date in trackedItems)
                {
                    _trackedLayers.Add(date);
                }

                return _trackedLayers.Count;
            }

            public bool TryGetUsed(out int used)
            {
                if (_schedulePart != null)
                {
                    used = Used(_schedulePart.Period);
                    return true;
                }
                used = 0;
                return false;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
            public bool TryGetUsed(out int used,IEntity targetPerson,DateTime date)
            {
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
 
                        IScenario scenario = new ScenarioRepository(uow).LoadDefaultScenario();
                        ScheduleRepository schedRep = new ScheduleRepository(uow);
                        PersonRepository persRep = new PersonRepository(uow);
                        IPerson person = persRep.Get((Guid)targetPerson.Id);

                        if (person.Period(date)!= null)
                        {

                            IScheduleDictionary result = schedRep.Find(new List<IPerson> {person},
                                                                       person.Period(date).Period, scenario);
                            result.ExtractAllScheduleData(this);
                            used = Used(person.Period(date).Period);
                            return true;

                        }

                }

               
                used = 0;
                return false;
            }
        
    
        
            public IList<DateTime> TrackedDateTimes
            {
                get { return _trackedLayers; }
            }

            public IScheduleRange ScheduleRange
            {
                get { return _scheduleRange; }
                set { _scheduleRange = value; }
            }

            public Description TrackingDescription
            {
                get { return _trackingDescription; }
                set { _trackingDescription = value; }
           }


            #region IScheduleExtractor Members

            public void AddSchedulePart(ISchedulePart schedulePart)
            {
                _schedulePart = schedulePart;
            }

            #endregion

            #region ITracker<int> Members

            int ITracker<int>.Used(DateTimePeriod dateTimePeriodToCheck, Description trackingDescription)
            {
                throw new NotImplementedException();
            }

          

            #endregion
    }

    
}
