using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic
{
    public interface ISaveSchedulePartService
    {
        void Save(IUnitOfWork unitOfWork, IScheduleDay scheduleDay);
    }

    public class SaveSchedulePartService : ISaveSchedulePartService
    {
        private readonly IScheduleDictionarySaver _scheduleDictionarySaver;
        private readonly IScheduleRepository _scheduleRepository;

        public SaveSchedulePartService(IScheduleDictionarySaver scheduleDictionarySaver, IScheduleRepository scheduleRepository)
        {
            _scheduleDictionarySaver = scheduleDictionarySaver;
            _scheduleRepository = scheduleRepository;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Save(IUnitOfWork unitOfWork, IScheduleDay scheduleDay)
        {
            var dic = (IReadOnlyScheduleDictionary)scheduleDay.Owner;
            dic.MakeEditable();

            var invalidList = dic.Modify(ScheduleModifier.Scheduler,
                                         scheduleDay,
                                         NewBusinessRuleCollection.Minimum(), new EmptyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));

            if (invalidList != null && invalidList.Any())
                throw new FaultException("Business rule violated: " + invalidList.First().Message);

            _scheduleDictionarySaver.MarkForPersist(unitOfWork, _scheduleRepository, dic.DifferenceSinceSnapshot());
        }
    }
}
