// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// DO NOT EDIT! This file was generated by CustomTasks.DependencyPropertyCodeGen
#include "pch.h"
#include "common.h"
#include "RevealBrush.h"

CppWinRTActivatableClassWithDPFactory(RevealBrush)

GlobalDependencyProperty RevealBrushProperties::s_AlwaysUseFallbackProperty{ nullptr };
GlobalDependencyProperty RevealBrushProperties::s_ColorProperty{ nullptr };
GlobalDependencyProperty RevealBrushProperties::s_StateProperty{ nullptr };
GlobalDependencyProperty RevealBrushProperties::s_TargetThemeProperty{ nullptr };

RevealBrushProperties::RevealBrushProperties()
{
    EnsureProperties();
}

void RevealBrushProperties::EnsureProperties()
{
    if (!s_AlwaysUseFallbackProperty)
    {
        s_AlwaysUseFallbackProperty =
            InitializeDependencyProperty(
                L"AlwaysUseFallback",
                winrt::name_of<bool>(),
                winrt::name_of<winrt::RevealBrush>(),
                false /* isAttached */,
                ValueHelper<bool>::BoxedDefaultValue(),
                &RevealBrush::OnPropertyChanged);
    }
    if (!s_ColorProperty)
    {
        s_ColorProperty =
            InitializeDependencyProperty(
                L"Color",
                winrt::name_of<winrt::Color>(),
                winrt::name_of<winrt::RevealBrush>(),
                false /* isAttached */,
                ValueHelper<winrt::Color>::BoxValueIfNecessary(RevealBrush::sc_defaultColor),
                &RevealBrush::OnPropertyChanged);
    }
    if (!s_StateProperty)
    {
        s_StateProperty =
            InitializeDependencyProperty(
                L"State",
                winrt::name_of<winrt::RevealBrushState>(),
                winrt::name_of<winrt::RevealBrush>(),
                true /* isAttached */,
                ValueHelper<winrt::RevealBrushState>::BoxValueIfNecessary(winrt::RevealBrushState::Normal),
                &RevealBrush::OnStatePropertyChanged);
    }
    if (!s_TargetThemeProperty)
    {
        s_TargetThemeProperty =
            InitializeDependencyProperty(
                L"TargetTheme",
                winrt::name_of<winrt::ApplicationTheme>(),
                winrt::name_of<winrt::RevealBrush>(),
                false /* isAttached */,
                ValueHelper<winrt::ApplicationTheme>::BoxValueIfNecessary(winrt::ApplicationTheme::Light),
                &RevealBrush::OnPropertyChanged);
    }
}

void RevealBrushProperties::ClearProperties()
{
    s_AlwaysUseFallbackProperty = nullptr;
    s_ColorProperty = nullptr;
    s_StateProperty = nullptr;
    s_TargetThemeProperty = nullptr;
}

void RevealBrushProperties::OnPropertyChanged(
    winrt::DependencyObject const& sender,
    winrt::DependencyPropertyChangedEventArgs const& args)
{
    auto owner = sender.as<winrt::RevealBrush>();
    winrt::get_self<RevealBrush>(owner)->OnPropertyChanged(args);
}

void RevealBrushProperties::AlwaysUseFallback(bool value)
{
    static_cast<RevealBrush*>(this)->SetValue(s_AlwaysUseFallbackProperty, ValueHelper<bool>::BoxValueIfNecessary(value));
}

bool RevealBrushProperties::AlwaysUseFallback()
{
    return ValueHelper<bool>::CastOrUnbox(static_cast<RevealBrush*>(this)->GetValue(s_AlwaysUseFallbackProperty));
}

void RevealBrushProperties::Color(winrt::Color const& value)
{
    static_cast<RevealBrush*>(this)->SetValue(s_ColorProperty, ValueHelper<winrt::Color>::BoxValueIfNecessary(value));
}

winrt::Color RevealBrushProperties::Color()
{
    return ValueHelper<winrt::Color>::CastOrUnbox(static_cast<RevealBrush*>(this)->GetValue(s_ColorProperty));
}

void RevealBrushProperties::SetState(winrt::UIElement const& target, winrt::RevealBrushState const& value)
{
    target.SetValue(s_StateProperty, ValueHelper<winrt::RevealBrushState>::BoxValueIfNecessary(value));
}

winrt::RevealBrushState RevealBrushProperties::GetState(winrt::UIElement const& target)
{
    return ValueHelper<winrt::RevealBrushState>::CastOrUnbox(target.GetValue(s_StateProperty));
}

void RevealBrushProperties::TargetTheme(winrt::ApplicationTheme const& value)
{
    static_cast<RevealBrush*>(this)->SetValue(s_TargetThemeProperty, ValueHelper<winrt::ApplicationTheme>::BoxValueIfNecessary(value));
}

winrt::ApplicationTheme RevealBrushProperties::TargetTheme()
{
    return ValueHelper<winrt::ApplicationTheme>::CastOrUnbox(static_cast<RevealBrush*>(this)->GetValue(s_TargetThemeProperty));
}
