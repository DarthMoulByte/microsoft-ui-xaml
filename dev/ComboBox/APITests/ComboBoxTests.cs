﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Common;
using MUXControlsTestApp.Utilities;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Linq;

#if USING_TAEF
using WEX.TestExecution;
using WEX.TestExecution.Markup;
using WEX.Logging.Interop;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
#endif

namespace Windows.UI.Xaml.Tests.MUXControls.ApiTests
{
    [TestClass]
    public class ComboBoxTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            TestUtilities.ClearVisualTreeRoot();
        }

        [TestMethod]
        public void VerifyComboBoxOverlayCornerRadius()
        {
            if (PlatformConfiguration.IsOSVersionLessThan(OSVersion.Redstone5))
            {
                Log.Warning("ComboBox corner radius is not available pre-rs5");
                return;
            }

            var comboBox = SetupComboBox();
            RunOnUIThread.Execute(() =>
            {
                comboBox.CornerRadius = new CornerRadius(2);
                comboBox.IsDropDownOpen = true;
            });
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                var background = TestUtilities.FindDescendents<Border>(comboBox).Where(e => e.Name == "Background").Single();
                Verify.AreEqual(new CornerRadius(2, 2, 2, 2), background.CornerRadius);

                var popup = VisualTreeHelper.GetOpenPopups(Window.Current).Last();
                var popupBorder = TestUtilities.FindDescendents<Border>(popup).Where(e => e.Name=="PopupBorder").Single();
                Verify.AreEqual(new CornerRadius(2, 2, 2, 2), popupBorder.CornerRadius);
            });
        }

        [TestMethod]
        public void VerifyComboBoxEditModeCornerRadius()
        {
            if (PlatformConfiguration.IsOSVersionLessThan(OSVersion.Redstone5))
            {
                Log.Warning("ComboBox corner radius is not available pre-rs5");
                return;
            }

            var comboBox = SetupComboBox();
            RunOnUIThread.Execute(() =>
            {
                comboBox.CornerRadius = new CornerRadius(2);
                comboBox.IsEditable = true;
                comboBox.IsDropDownOpen = true;
            });
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                var editableText = TestUtilities.FindDescendents<TextBox>(comboBox).Where(e => e.Name == "EditableText").Single();
                Verify.AreEqual(new CornerRadius(2, 2, 0, 0), editableText.CornerRadius);

                var popup = VisualTreeHelper.GetOpenPopups(Window.Current).Last();
                var popupBorder = TestUtilities.FindDescendents<Border>(popup).Where(e => e.Name == "PopupBorder").Single();
                Verify.AreEqual(new CornerRadius(0, 0, 2, 2), popupBorder.CornerRadius);
            });
        }

        private ComboBox SetupComboBox()
        {
            ComboBox comboBox = null;
            RunOnUIThread.Execute(() =>
            {
                comboBox = new ComboBox();
                comboBox.Items.Add("Item 1");
                comboBox.Items.Add("Item 2");
                comboBox.Items.Add("Item 3");
                comboBox.Items.Add("Item 4");
                comboBox.Items.Add("Item 5");
                comboBox.Items.Add("Item 6");
            });
            TestUtilities.SetAsVisualTreeRoot(comboBox);
            Verify.IsNotNull(comboBox);
            return comboBox;
        }

    }
}
