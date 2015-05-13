﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class UnitOfWorkAspectTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<MutableFakeCurrentHttpContext>().For<ICurrentHttpContext>();
			system.AddService<TheServiceImpl>();
		}

		public TheServiceImpl TheService;
		public MutableFakeCurrentHttpContext HttpContext;
		public IPersonRepository PersonRepository;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ISiteRepository SiteRepository;

		public class TheServiceImpl
		{
			private readonly ICurrentUnitOfWork _uow;

			public TheServiceImpl(ICurrentUnitOfWork uow)
			{
				_uow = uow;
			}

			[UnitOfWork]
			public virtual void Does(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}

		}

		[Test]
		public void ShouldQuery()
		{
			IEnumerable<IPerson> persons = null;

			TheService.Does(uow =>
			{
				persons = PersonRepository.LoadAll();
			});

			persons.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersist()
		{
			IEnumerable<IPerson> persons = null;
			var person = PersonFactory.CreatePerson();

			TheService.Does(uow =>
			{
				PersonRepository.Add(person);
			});
			TheService.Does(uow =>
			{
				persons = PersonRepository.LoadAll();
			});

			persons.Where(x => x.Id.Equals(person.Id.Value)).Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotPersistOnException()
		{
			IEnumerable<IPerson> persons = null;
			var person = PersonFactory.CreatePerson();

			try
			{
				TheService.Does(uow =>
				{
					PersonRepository.Add(person);
					throw new Exception();
				});
			}
			catch (Exception)
			{
			}
			TheService.Does(uow =>
			{
				persons = PersonRepository.LoadAll();
			});

			persons.Where(x => x.Id.Equals(person.Id.Value)).Should().Be.Empty();
		}

		[Test]
		public void ShouldFilterOnBusinessUnitFromHttpQueryString()
		{
			var businessUnit1 = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var businessUnit2 = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var site1 = SiteFactory.CreateSimpleSite();
			site1.SetBusinessUnit(businessUnit1);
			var site2 = SiteFactory.CreateSimpleSite();
			site2.SetBusinessUnit(businessUnit2);
			TheService.Does(uow =>
			{
				BusinessUnitRepository.Add(businessUnit1);
				BusinessUnitRepository.Add(businessUnit2);
				SiteRepository.Add(site1);
				SiteRepository.Add(site2);
			});

			IEnumerable<ISite> sites = null;
			var queryStringParams = new NameValueCollection { { "BusinessUnitId", businessUnit2.Id.Value.ToString() } };
			HttpContext.SetContext(new FakeHttpContext(null, null, null, queryStringParams, null, null));
			TheService.Does(uow =>
			{
				sites = SiteRepository.LoadAll();
			});

			sites.Single().Id.Should().Be(site2.Id.Value);
		}

		[Test]
		public void ShouldFilterOnBusinessUnitFromHttpHeader()
		{
			var businessUnit1 = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var businessUnit2 = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var site1 = SiteFactory.CreateSimpleSite();
			site1.SetBusinessUnit(businessUnit1);
			var site2 = SiteFactory.CreateSimpleSite();
			site2.SetBusinessUnit(businessUnit2);
			TheService.Does(uow =>
			{
				BusinessUnitRepository.Add(businessUnit1);
				BusinessUnitRepository.Add(businessUnit2);
				SiteRepository.Add(site1);
				SiteRepository.Add(site2);
			});

			IEnumerable<ISite> sites = null;
			var headers = new NameValueCollection { { "X-Business-Unit-Filter", businessUnit2.Id.Value.ToString() } };
			HttpContext.SetContext(new FakeHttpContext(null, null, null, null, null, null, null, headers));
			TheService.Does(uow =>
			{
				sites = SiteRepository.LoadAll();
			});

			sites.Single().Id.Should().Be(site2.Id.Value);
		}

		[Test, Ignore]
		public void ShouldFilterOnBusinessUnitFromHttpHeaderOverQueryString()
		{
			Assert.Fail();
		}

	}

}
