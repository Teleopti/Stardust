using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{

    [TestFixture]
    public class ModifySelectionPresenterTest
    {
        private ModifySelectionPresenter target;
        private MockRepository mocks;
        private IModifySelectionView view;
        private ModifyCalculator model;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            view = mocks.StrictMock<IModifySelectionView>();
            List<double> list = new List<double>() { 1000, 2000, 3000 };

            model = new ModifyCalculator(list); 

            target = new ModifySelectionPresenter(view, model);
        }

        [Test]
        public void VerifyCanInitialize()
        {
            using (mocks.Record())
            {
                view.InputState = 0;
                view.InputType = "0";
                view.InputSmoothValue = "0";
                view.Sum = model.Sum;
                view.ModifiedSum = model.ModifiedSum;
                view.ChosenAmount = model.ChosenAmount;
                view.Average = model.Average;
                view.StandardDev = Math.Round(model.StandardDev, 1);
                view.InputPercent = model.Sum.ToString(CultureInfo.CurrentCulture);
            }
            using (mocks.Playback())
            {
                target.Initialize();
            }
        }

        [Test]
        public void VerifyCanUndo()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputType).Return("0").Repeat.Any();
                view.InputPercent = "6000";
            }

            using (mocks.Playback())
            {
                target.Undo();
            }
        }

        [Test]
        public void VerifyCanCheckOriginalResult()
        {
            using (mocks.Record())
            {
                model.OriginalValues[0] = 1000;
                model.OriginalValues[1] = 2000;
                model.OriginalValues[2] = 3000;
            }

            using (mocks.Playback())
            {
                target.OriginalResult();
            }
        }

        [Test]
        public void VerifyCanCheckResult()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputSmoothValue).Return("1").Repeat.Any();
                model.ModifiedValues[0] = 1000;
                model.ModifiedValues[1] = 2000;
                model.ModifiedValues[2] = 3000;
            }

            using (mocks.Playback())
            {
                target.Result();
            }
        }

        [Test]
        public void VerifyCanAccept()
        {
            using (mocks.Record())
            {
                Expect.Call(view.Close).Repeat.Any();
                view.SetDialogResult(DialogResult.OK);
            }

            using (mocks.Playback())
            {
                target.Accept();
            }
        }

        [Test]
        public void VerifyCanCancel()
        {
            using (mocks.Record())
            {
                Expect.Call(view.Close).Repeat.Any();
                view.SetDialogResult(DialogResult.Cancel);
            }

            using (mocks.Playback())
            {
                target.Cancel();
            }
        }

        [Test]
        public void VerifyTotalTextFieldUpdateValues()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputPercent).Return("-50").Repeat.Any();
                Expect.Call(view.InputSmoothValue).Return("9").Repeat.Any();
                Expect.Call(view.InputType).Return("0").Repeat.Any();
                view.ModifiedSum = -50;
                view.Average = -16.7;
                view.StandardDev = 0;
                view.InputState = 0;
            }

            using (mocks.Playback())
            {
                target.UpdateInputTotal();
            }
        }

        [Test]
        public void VerifySmooth()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputSmoothValue).Return("1").Repeat.Any();
                view.StandardDev = 2160.2;
            }

            using (mocks.Playback())
            {
                target.UpdateSmoothList();
            }
        }

        [Test]
        public void VerifyUpdateNewSum()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputPercent).Return("50").Repeat.Any();
                Expect.Call(view.InputSmoothValue).Return("3").Repeat.Any();
                Expect.Call(view.InputType).Return("0").Repeat.Any();
                Expect.Call(view.InputState).Return(0).Repeat.Any();
                view.InputState = 0;
                view.InputPercent = "9000";
            }

            using (mocks.Playback())
            {
                target.UpdateNewSum();
            }
        }

        [Test]
        public void VerifyUpdateNewSumTypeTwo()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputPercent).Return("50").Repeat.Any();
                Expect.Call(view.InputSmoothValue).Return("3").Repeat.Any();
                Expect.Call(view.InputType).Return("1").Repeat.Any();
                Expect.Call(view.InputState).Return(1).Repeat.Any();
                view.InputState = 1;
                view.InputPercent = "-99";
            }

            using (mocks.Playback())
            {
                target.UpdateNewSum();
            }
        }

        [Test]
        public void VerifyUpdateInputTotal()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputPercent).Return("50").Repeat.Any();
                Expect.Call(view.InputSmoothValue).Return("3").Repeat.Any();
                Expect.Call(view.InputType).Return("0").Repeat.Any();
                Expect.Call(view.InputState).Return(0).Repeat.Any();
                view.ModifiedSum = 50;
                view.Average = 16.7;
                view.StandardDev = 3.4;
                view.InputState = 0;
            }

            using (mocks.Playback())
            {
                target.UpdateInputTotal();
            }
        }

        [Test]
        public void VerifyUpdateInputTotalTypeTwo()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputPercent).Return("50").Repeat.Any();
                Expect.Call(view.InputSmoothValue).Return("3").Repeat.Any();
                Expect.Call(view.InputType).Return("1").Repeat.Any();
                Expect.Call(view.InputState).Return(1).Repeat.Any();
                view.ModifiedSum = 9000;
                view.Average = 3000;
                view.StandardDev = 612.4;
                view.InputState = 1;
            }

            using (mocks.Playback())
            {
                target.UpdateInputTotal();
            }
        }

        [Test]
        public void VerifyUpdateSmoothListNone()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputSmoothValue).Return("None").Repeat.Any();
                model.SmoothenValues(1);
                model.ModifiedValues[0] = 1000;
                model.ModifiedValues[1] = 2000;
                model.ModifiedValues[2] = 3000;
                view.StandardDev = 2160.2;
                view.InputSmoothValue = "0";
            }

            using (mocks.Playback())
            {
                target.UpdateSmoothList();
            }
        }

        [Test]
        public void VerifyUpdateSmoothListThree()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputSmoothValue).Return("3").Repeat.Any();
                model.SmoothenValues(3);
                model.SmoothenedValues[0] = 1500;
                model.SmoothenedValues[1] = 2000;
                model.SmoothenedValues[2] = 2500;
                view.StandardDev = 2041.2;
            }

            using (mocks.Playback())
            {
                target.UpdateSmoothList();
            }
        }

        [Test]
        public void VerifyUpdateSmoothListFive()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputSmoothValue).Return("5").Repeat.Any();
                model.SmoothenValues(5);
                model.ModifiedValues[0] = 2000;
                model.ModifiedValues[1] = 2000;
                model.ModifiedValues[2] = 2000;
                view.StandardDev = 2000;
            }

            using (mocks.Playback())
            {
                target.UpdateSmoothList();
            }
        }

        [Test]
        public void VerifyUpdateSmoothListSeven()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputSmoothValue).Return("7").Repeat.Any();
                model.SmoothenValues(7);
                model.ModifiedValues[0] = 2000;
                model.ModifiedValues[1] = 2000;
                model.ModifiedValues[2] = 2000;
                view.StandardDev = 2000;
            }

            using (mocks.Playback())
            {
                target.UpdateSmoothList();
            }
        }

        [Test]
        public void VerifyUpdateSmoothList()
        {
            using (mocks.Record())
            {
                Expect.Call(view.InputSmoothValue).Return("9").Repeat.Any();
                model.SmoothenValues(9);
                model.ModifiedValues[0] = 2000;
                model.ModifiedValues[1] = 2000;
                model.ModifiedValues[2] = 2000;
                view.StandardDev = 2000;
            }

            using (mocks.Playback())
            {
                target.UpdateSmoothList();
            }
        }
    }
}
