using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class AbsenceFactory
    {
        public static Absence CreateAbsence(string name, string shortName,
                                            Color color)
        {
            InParameter.NotNull("name", name);
            InParameter.NotNull("shortName", shortName);
            InParameter.NotNull("color", color);
            Absence ret = new Absence();
            ret.Description = new Description(name,shortName);
            ret.DisplayColor = color;
            ret.Priority = 145;
            return ret;
        }

        public static Absence CreateRequestableAbsence(string name, string shortName,
                                         Color color)
        {
            InParameter.NotNull("name", name);
            InParameter.NotNull("shortName", shortName);
            InParameter.NotNull("color", color);
            Absence ret = new Absence();
            ret.Description = new Description(name, shortName);
            ret.DisplayColor = color;
            ret.Priority = 145;
            ret.Requestable = true;
            return ret;
        }

        public static IAbsence CreateAbsence(string name)
        {
            InParameter.NotNull("name", name);
            IAbsence ret = new Absence();
            ret.Description = new Description(name,name.Substring(0, 2));
            ret.DisplayColor = Color.Red;
            return ret;
        }

		public static IAbsence CreateAbsenceWithTracker(string name, ITracker tracker)
		{
			InParameter.NotNull("name", name);
			IAbsence ret = new Absence();
			ret.Description = new Description(name, name.Substring(0, 2));
			ret.DisplayColor = Color.Red;
			ret.Tracker = tracker;
			return ret;
		}

		public static IAbsence CreateAbsenceWithId()
	    {
		    var ret = CreateAbsence("absence");
			ret.SetId(Guid.NewGuid());
			return ret;
		}
    }
}