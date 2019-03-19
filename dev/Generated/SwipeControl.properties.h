// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// DO NOT EDIT! This file was generated by CustomTasks.DependencyPropertyCodeGen
#pragma once

class SwipeControlProperties
{
public:
    SwipeControlProperties();

    void BottomItems(winrt::SwipeItems const& value);
    winrt::SwipeItems BottomItems();

    void LeftItems(winrt::SwipeItems const& value);
    winrt::SwipeItems LeftItems();

    void RightItems(winrt::SwipeItems const& value);
    winrt::SwipeItems RightItems();

    void TopItems(winrt::SwipeItems const& value);
    winrt::SwipeItems TopItems();

    static winrt::DependencyProperty BottomItemsProperty() { return s_BottomItemsProperty; }
    static winrt::DependencyProperty LeftItemsProperty() { return s_LeftItemsProperty; }
    static winrt::DependencyProperty RightItemsProperty() { return s_RightItemsProperty; }
    static winrt::DependencyProperty TopItemsProperty() { return s_TopItemsProperty; }

    static GlobalDependencyProperty s_BottomItemsProperty;
    static GlobalDependencyProperty s_LeftItemsProperty;
    static GlobalDependencyProperty s_RightItemsProperty;
    static GlobalDependencyProperty s_TopItemsProperty;

    static void EnsureProperties();
    static void ClearProperties();

    static void OnPropertyChanged(
        winrt::DependencyObject const& sender,
        winrt::DependencyPropertyChangedEventArgs const& args);
};
