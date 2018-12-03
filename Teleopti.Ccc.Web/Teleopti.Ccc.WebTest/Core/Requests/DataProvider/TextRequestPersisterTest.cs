using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.Exceptions;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;


namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	[DomainTest] 
	[WebTest] 
	[RequestsTest] 
	public class TextRequestPersisterTest : IIsolateSystem
	{
		public ITextRequestPersister Target;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeLoggedOnUser LoggedOnUser;

		[Test]
		public void ShouldAddTextRequest()
		{ 
			var form = new TextRequestForm();
			
			Target.Persist(form);

			PersonRequestRepository.LoadAll().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldDelete()
		{
			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser()){Request = new TextRequest(new DateTimePeriod())}.WithId();
			PersonRequestRepository.Add(personRequest);
			
			Target.Delete(personRequest.Id.Value);

			PersonRequestRepository.LoadAll().Should().Have.Count.EqualTo(0);
		}		
		
		[Test]
		public void ShouldSetExchangeOfferStatusWhenDelete()
		{
			var currentShift = ScheduleDayFactory.Create(DateOnly.Today.AddDays(1));
			var offer = new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.PendingAdminApproval);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>()) {Offer = offer};
			var personRequest = new PersonRequest(new Person(),shiftTradeRequest).WithId();
			
			PersonRequestRepository.Add(personRequest);

			Target.Delete(personRequest.Id.Value);

			offer.Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.Pending);
			PersonRequestRepository.LoadAll().Should().Have.Count.EqualTo(0);
		}		
		
		[Test]
		public void ShouldNotChangeCompletedExchangeOfferStatusWhenDelete()
		{
			var currentShift = ScheduleDayFactory.Create(DateOnly.Today.AddDays(1));
			var offer = new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Completed);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>()) { Offer = offer };
			var personRequest = new PersonRequest(new Person(), shiftTradeRequest).WithId();

			PersonRequestRepository.Add(personRequest);

			Target.Delete(personRequest.Id.Value);

			offer.Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.Completed);
			PersonRequestRepository.LoadAll().Should().Have.Count.EqualTo(0);
		}		
		
		[Test]
		public void ShouldNotChangeInvalidExchangeOfferStatusWhenDelete()
		{
			var currentShift = ScheduleDayFactory.Create(DateOnly.Today.AddDays(1));
			var offer = new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Invalid);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>()) { Offer = offer };
			var personRequest = new PersonRequest(new Person(), shiftTradeRequest).WithId();

			PersonRequestRepository.Add(personRequest);

			Target.Delete(personRequest.Id.Value);

			offer.Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.Invalid);
			PersonRequestRepository.LoadAll().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldThrowHttp404OIfTextRequestDoesNotExists()
		{
			var id = Guid.NewGuid();
			
			var exception = Assert.Throws<CustomMessageException>(() => Target.Delete(id));
			exception.GetHttpCode().Should().Be(404);
		}
		
		[Test]
		public void ShouldUpdateExistingTextRequest()
		{
			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser(),
				new TextRequest(new DateTimePeriod(2017, 1, 1, 2017, 1, 1))).WithId();
			
			var form = new TextRequestForm {Message = "test"};
			form.EntityId = personRequest.Id.Value;

			PersonRequestRepository.Add(personRequest);

			Target.Persist(form);

			personRequest.GetMessage(new NoFormatting()).Should().Be.EqualTo("test");
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLinkProvider>().For<ILinkProvider>();
			isolate.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
			isolate.UseTestDouble<FakeLicensedFunctionProvider>().For<ILicensedFunctionsProvider>();

			var currentBusinessUnit = new SpecificBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			isolate.UseTestDouble(currentBusinessUnit).For<ICurrentBusinessUnit>();
			var dataSource = new FakeCurrentDatasource("Test");
			isolate.UseTestDouble(dataSource).For<ICurrentDataSource>();
		}
	}
}