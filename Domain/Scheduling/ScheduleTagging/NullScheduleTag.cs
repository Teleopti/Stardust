using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ScheduleTagging
{
    public sealed class NullScheduleTag : IScheduleTag
    {
		internal static readonly Lazy<NullScheduleTag> instance = new Lazy<NullScheduleTag>(()=>new NullScheduleTag());

        public string Description
        {
            get { return UserTexts.Resources.DefaultTag; }
            set { }
        }

        public static NullScheduleTag Instance
        {
            get { return instance.Value; }
        }

        public bool Equals(IEntity other)
        {
            if (other is NullScheduleTag)
                return true;

            return false;
        }

        public Guid? Id
        {
            get { return null; }
        }

        public void SetId(Guid? newId)
        {
        }

        public void ClearId()
        {
        }

        public IPerson CreatedBy
        {
            get { return null; }
        }

        public DateTime? CreatedOn
        {
            get { return null; }
        }

        public IPerson UpdatedBy
        {
            get { return null; }
        }

        public DateTime? UpdatedOn
        {
            get { return null; }
        }

        public bool IsDeleted
        {
            get { return false; }
        }

        public void SetDeleted()
        {
        }
    }
}