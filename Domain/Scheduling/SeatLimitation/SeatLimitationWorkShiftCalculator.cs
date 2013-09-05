//using System;
//using System.Collections.Generic;
//using Teleopti.Interfaces.Domain;

//namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
//{
//    public interface ISeatLimitationWorkShiftCalculator
//    {
//        double CalculateShiftValue(IVirtualSchedulePeriod virtualPeriod, IVisualLayerCollection layers, IDictionary<ISkill, ISkillStaffPeriodDictionary> skillStaffPeriods);
//    }

//    public class SeatLimitationWorkShiftCalculator : ISeatLimitationWorkShiftCalculator
//    {
//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
//        public double CalculateShiftValue(IVirtualSchedulePeriod virtualPeriod, IVisualLayerCollection layers, IDictionary<ISkill, ISkillStaffPeriodDictionary> skillStaffPeriods)
//        {
//            if (virtualPeriod.PersonPeriod.PersonMaxSeatSkillCollection.Count == 0)
//                return 0;

//            var activityRequiresSeat = false;
//            foreach (var layer in layers)
//            {
//                var activity = (IActivity)layer.Payload;
//                if (activity != null && !activity.RequiresSeat) continue;
//                activityRequiresSeat = true;
//                break;
//            }

//            // no layer requires seat
//            if (!activityRequiresSeat)
//                return 0;

//            double result = 0;

//            foreach (IVisualLayer layer in layers)
//            {
//                var activity = (IActivity)layer.Payload;

//                if (activity == null) continue;

//                if (!activity.RequiresSeat) continue;

//                foreach (var key in skillStaffPeriods.Keys)
//                {
//                    var layerStart = layer.Period.StartDateTime;
//                    var layerEnd = layer.Period.EndDateTime;
//                    ISkillStaffPeriodDictionary dictionary;
//                    if (skillStaffPeriods.TryGetValue(key, out dictionary))
//                    {
//                        var resolution = GetResolution(dictionary.Values);

//                        DateTime currentStart = layerStart.Date.Add(
//                                TimeHelper.FitToDefaultResolution(layerStart.TimeOfDay, resolution));
//                        if (currentStart > layerStart)
//                        {
//                            currentStart = currentStart.AddMinutes(-resolution);
//                        }
//                        while (currentStart < layerEnd)
//                        {
//                            ISkillStaffPeriod currentStaffPeriod;
//                            if (dictionary.TryGetValue(new DateTimePeriod(currentStart, currentStart.AddMinutes(resolution)), out currentStaffPeriod))
//                            {
//                                var intersection = currentStaffPeriod.Period.Intersection(layer.Period);
//                                if (intersection.HasValue)
//                                {
//                                    var partOffPeriod = intersection.Value.ElapsedTime().TotalSeconds / currentStaffPeriod.Period.ElapsedTime().TotalSeconds;
//                                    if (currentStaffPeriod.Payload.CalculatedUsedSeats + partOffPeriod > currentStaffPeriod.Payload.MaxSeats)
//                                        result += -10000;
									
//                                    //result += intersection.Value.ElapsedTime().TotalSeconds / currentStaffPeriod.Period.ElapsedTime().TotalSeconds;
//                                }
//                            }
//                            currentStart = currentStart.AddMinutes(resolution);
//                        }
//                    }
//                }
				
//            }
//            return result;
//        }

//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
//        public static int GetResolution(ICollection<ISkillStaffPeriod> staffPeriods)
//        {
//            int resolution = 15;
//            foreach (ISkillStaffPeriod pair in staffPeriods)
//            {
//                resolution = (int)((pair.Period.EndDateTime - pair.Period.StartDateTime).TotalMinutes);
//                break;
//            }
//            return resolution;
//        }
//    }
//}