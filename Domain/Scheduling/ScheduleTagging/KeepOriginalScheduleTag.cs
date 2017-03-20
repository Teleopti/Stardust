using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ScheduleTagging
{
    public sealed class KeepOriginalScheduleTag : IScheduleTag
    {
	    private static readonly Guid keepOriginalId = new Guid("00000000-0000-0000-0000-111111111111");

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
			get { return UserTexts.Resources.KeepTag;  }
			set { }
        }

        KeepOriginalScheduleTag()
        {
        }

        public static KeepOriginalScheduleTag Instance => Nested.instance;
		
	    public bool Equals(IEntity other)
        {
            if (other is KeepOriginalScheduleTag)
                return true;

            return false;
        }

        public Guid? Id => keepOriginalId;

	    public void SetId(Guid? newId)
        {
        }

        public void ClearId()
        {
        }

        public IPerson CreatedBy => null;

	    public DateTime? CreatedOn => null;

	    public IPerson UpdatedBy => null;

	    public DateTime? UpdatedOn => null;

	    public bool IsDeleted => false;

	    public void SetDeleted()
        {
        }
    }
}