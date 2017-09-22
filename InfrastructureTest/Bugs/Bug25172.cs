using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
	[TestFixture]
	public class Bug25172 : DatabaseTest
	{
		[Test]
		public void AggregatesImplementingIVersionedShouldBeVersionedInMapping()
		{
			var illegalTypes = new List<Type>();
			foreach (var classMetadata in Session.SessionFactory.GetAllClassMetadata())
			{
				var type = Type.GetType(classMetadata.Key + ", Teleopti.Ccc.Domain");
				if (!classMetadata.Value.IsVersioned && typeof(IVersioned).IsAssignableFrom(type))
				{
					illegalTypes.Add(type);
				}
			}
			if (illegalTypes.Count > 0)
			{
				var typeOutPut = Environment.NewLine + Environment.NewLine;
				foreach (var illegalType in illegalTypes)
				{
					typeOutPut += illegalType + Environment.NewLine;
				}
				Assert.Fail(@"The following types implements IVersioned but is not versioned in the database! " + typeOutPut);
			}
		}

	}
}