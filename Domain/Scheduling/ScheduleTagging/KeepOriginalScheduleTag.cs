using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ScheduleTagging
{
    public sealed class KeepOriginalScheduleTag : IScheduleTag
    {
        private class Nested
        {
            private Nested(){}
            //// Explicit static constructor to tell C# compiler
            //// not to mark type as beforefieldinit
            //static Nested()
            //{
            //}

            internal static readonly KeepOriginalScheduleTag instance = new KeepOriginalScheduleTag();
        }

        public string Description
        {
			get { return "<Keep existing>"; }
			set { }
        }

        KeepOriginalScheduleTag()
        {
        }

        public static KeepOriginalScheduleTag Instance
        {
            get { return KeepOriginalScheduleTag.Nested.instance; }
        }


        public bool Equals(IEntity other)
        {
            if (other is KeepOriginalScheduleTag)
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
            return;
        }
    }
}