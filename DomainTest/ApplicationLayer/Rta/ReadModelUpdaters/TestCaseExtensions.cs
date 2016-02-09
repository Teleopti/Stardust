using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class TestCase
	{
		public string Name;
		public Guid Team;
		public Guid OriginTeam;
		public Guid DestinationTeam;
		public Guid Site;
		public Guid OriginSite;
		public Guid DestinationSite;
		public IEnumerable<IEvent> Events;
	}

	public static class TestCaseExtensions
	{
		public static IEnumerable TestCases(
			this IEnumerable<IEnumerable<IEvent>> permutations, 
			IEnumerable<IEvent> setup,
			Action<TestCase> decorate)
		{
			return permutations
				.Select(p =>
				{
					var es = p.ToArray();
					var info = new TestCase
					{
						Name = TestCaseName(es),
						Events = setup.EmptyIfNull().Concat(es).ToArray()
					};
					if (decorate != null)
						decorate(info);
					return new TestCaseData(info).SetName(info.Name);
				});
		}
		
		public static string TestCaseName(this IEnumerable<object> objects)
		{
			return objects
				.Select(e => e.GetType().Name)
				.Aggregate((current, next) => current + ", " + next);
		}

	}

}