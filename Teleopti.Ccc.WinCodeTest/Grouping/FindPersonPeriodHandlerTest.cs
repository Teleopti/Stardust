using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;


namespace Teleopti.Ccc.WinCodeTest.Grouping
{
    [TestFixture]
    public class FindPersonPeriodHandlerTest
    {
		[Test]
		public void ShouldReturnFalseWhenFromIsGreaterThanTo()
		{
			var findPersonsModel = new FindPersonsModel(new IPerson[0]);
			var findPersonsView = MockRepository.GenerateMock<IFindPersonsView>();

			var target = new FindPersonPeriodHandler(findPersonsModel, findPersonsView);

			findPersonsView.Stub(x => x.FromDate).Return(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc));
			findPersonsView.Stub(x => x.ToDate).Return(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc));

			Assert.IsFalse(target.CheckPeriod());
		}

		[Test]
        public void ShouldReturnTrueWhenFromIsLessThanTo()
        {
			var findPersonsModel = new FindPersonsModel(new IPerson[0]);
			var findPersonsView = MockRepository.GenerateMock<IFindPersonsView>();

			var target = new FindPersonPeriodHandler(findPersonsModel, findPersonsView);

			findPersonsView.Stub(x => x.FromDate).Return(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc));
			findPersonsView.Stub(x => x.ToDate).Return(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc));

			Assert.IsTrue(target.CheckPeriod());
		}

        [Test]
        public void ShouldSetErrorWhenFromIsGreaterThanTo()
		{
			var findPersonsModel = new FindPersonsModel(new IPerson[0]);
			var findPersonsView = MockRepository.GenerateMock<IFindPersonsView>();

			var target = new FindPersonPeriodHandler(findPersonsModel, findPersonsView);

			findPersonsView.Stub(x => x.FromDate).Return(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc));
			findPersonsView.Stub(x => x.ToDate).Return(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc));

			target.FromDateChanged();

			findPersonsView.AssertWasCalled(x => x.SetErrorOnStartDate(UserTexts.Resources.StartDateMustBeSmallerThanEndDate));
			findPersonsView.AssertWasCalled(x => x.TextBoxFindEnabled = false);
		}

        [Test]
        public void ShouldSetErrorWhenToIsLessThanFrom()
		{
			var findPersonsModel = new FindPersonsModel(new IPerson[0]);
			var findPersonsView = MockRepository.GenerateMock<IFindPersonsView>();

			var target = new FindPersonPeriodHandler(findPersonsModel, findPersonsView);

			findPersonsView.Stub(x => x.FromDate).Return(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc));
			findPersonsView.Stub(x => x.ToDate).Return(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc));

			target.ToDateChanged();

			findPersonsView.AssertWasCalled(x => x.SetErrorOnEndDate(UserTexts.Resources.StartDateMustBeSmallerThanEndDate));
			findPersonsView.AssertWasCalled(x => x.TextBoxFindEnabled = false);
		}

        [Test]
        public void ShouldRebuildIndexWhenFromIsLessThanTo()
		{
			var findPersonsModel = new FindPersonsModel(new IPerson[0]);
			var findPersonsView = MockRepository.GenerateMock<IFindPersonsView>();

			var target = new FindPersonPeriodHandler(findPersonsModel, findPersonsView);

			findPersonsView.Stub(x => x.FromDate).Return(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc));
			findPersonsView.Stub(x => x.ToDate).Return(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc));

			target.FromDateChanged();

			findPersonsModel.FromDate.Should().Be.EqualTo(new DateOnly(2011, 9, 9));
			findPersonsView.AssertWasCalled(x => x.ClearDateErrors());
			findPersonsView.AssertWasCalled(x => x.TextBoxFindEnabled = true);
		}

        [Test]
        public void ShouldRebuildIndexWhenToIsGreaterThanFrom()
		{
			var findPersonsModel = new FindPersonsModel(new IPerson[0]);
			var findPersonsView = MockRepository.GenerateMock<IFindPersonsView>();

			var target = new FindPersonPeriodHandler(findPersonsModel, findPersonsView);

			findPersonsView.Stub(x => x.FromDate).Return(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc));
			findPersonsView.Stub(x => x.ToDate).Return(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc));
			
			target.FromDateChanged();
			target.ToDateChanged();

			findPersonsModel.ToDate.Should().Be.EqualTo(new DateOnly(2011, 10, 10));
			findPersonsView.AssertWasCalled(x => x.ClearDateErrors());
			findPersonsView.AssertWasCalled(x => x.TextBoxFindEnabled = true);
		}
    }
}
