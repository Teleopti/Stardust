﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Ccc.Rta.Server.Resolvers;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core.Server.Resolvers
{
    [TestFixture]
    public class DataSourceResolverTest
    {
	    [Test]
	    public void TryResolveId_ShouldInitializeDictionary()
	    {
			int datasource;
			var databaseReader = MockRepository.GenerateStub<IDatabaseReader>();
		    var target = new DataSourceResolver(databaseReader);

		    var dictionary = new Dictionary<string, int> {{"1234", 4}};
		    databaseReader.Stub(d => d.LoadDatasources()).Return(new ConcurrentDictionary<string, int>(dictionary));
			
			
			var result = target.TryResolveId("1234", out datasource);
			result.Should().Be.True();
		    datasource.Should().Be(4);
	    }
	}
}
