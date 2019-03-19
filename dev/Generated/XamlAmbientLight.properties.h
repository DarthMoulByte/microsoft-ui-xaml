// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// DO NOT EDIT! This file was generated by CustomTasks.DependencyPropertyCodeGen
#pragma once

class XamlAmbientLightProperties
{
public:
    XamlAmbientLightProperties();

    void Color(winrt::Color const& value);
    winrt::Color Color();

    static void SetIsTarget(winrt::DependencyObject const& target, bool value);
    static bool GetIsTarget(winrt::DependencyObject const& target);

    static winrt::DependencyProperty ColorProperty() { return s_ColorProperty; }
    static winrt::DependencyProperty IsTargetProperty() { return s_IsTargetProperty; }

    static GlobalDependencyProperty s_ColorProperty;
    static GlobalDependencyProperty s_IsTargetProperty;

    static void EnsureProperties();
    static void ClearProperties();

    static void OnPropertyChanged(
        winrt::DependencyObject const& sender,
        winrt::DependencyPropertyChangedEventArgs const& args);
};
