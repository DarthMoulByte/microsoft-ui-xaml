﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Windows.UI.Xaml.Controls;

#if !BUILD_WINDOWS
using NavigationViewDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewItemSeparator = Microsoft.UI.Xaml.Controls.NavigationViewItemSeparator;
using NavigationViewDisplayModeChangedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs;
#endif

namespace MUXControlsTestApp
{
    [TopLevelTestPage(Name = "NavigationView", Icon = "NavigationView.png")]
    public sealed partial class NavigationViewCaseBundle : TestPage
    {
        public NavigationViewCaseBundle()
        {
            this.InitializeComponent();
            NavigationViewPage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewPage), 0); };
            NavigationViewRS4Page.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewRS4Page), 0); };
            NavigationViewTopNavPage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewTopNavPage), 0); };
            NavigationViewTopNavOnlyPage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewTopNavOnlyPage), 0); };
            NavigationViewTopNavStorePage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewTopNavStorePage), 0); };
            NavigateToSelectedItemEdgeCasePage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewSelectedItemEdgeCasePage), 0); };
            NavigateToInitPage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewInitPage), 0); };
            NavigateToStretchPage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewStretchPage), 0); };
            NavigateToItemTemplatePage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewItemTemplatePage), 0); };
            NavigateToRS3Page.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewRS3Page), 0); };
            NavigateToTopNavPage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewTopNavStorePage), 0); };
            NavigateToAnimationPage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewAnimationPage), 0); };
            NavigateToIsPaneOpenPage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewIsPaneOpenPage), 0); };
            NavigateToMinimalPage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewMinimalPage), 0); };
            NavigateToCustomThemeResourcesPage.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewCustomThemeResourcesPage), 0); };
            NavigationViewBlankPage1.Click += delegate { Frame.NavigateWithoutAnimation(typeof(NavigationViewBlankPage1), 0); };
        }
    }
}
