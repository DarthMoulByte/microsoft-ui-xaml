// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// DO NOT EDIT! This file was generated by CustomTasks.DependencyPropertyCodeGen
#pragma once

class ColorSpectrumProperties
{
public:
    ColorSpectrumProperties();

    void Color(winrt::Color const& value);
    winrt::Color Color();

    void Components(winrt::ColorSpectrumComponents const& value);
    winrt::ColorSpectrumComponents Components();

    void HsvColor(winrt::float4 const& value);
    winrt::float4 HsvColor();

    void MaxHue(int value);
    int MaxHue();

    void MaxSaturation(int value);
    int MaxSaturation();

    void MaxValue(int value);
    int MaxValue();

    void MinHue(int value);
    int MinHue();

    void MinSaturation(int value);
    int MinSaturation();

    void MinValue(int value);
    int MinValue();

    void Shape(winrt::ColorSpectrumShape const& value);
    winrt::ColorSpectrumShape Shape();

    static winrt::DependencyProperty ColorProperty() { return s_ColorProperty; }
    static winrt::DependencyProperty ComponentsProperty() { return s_ComponentsProperty; }
    static winrt::DependencyProperty HsvColorProperty() { return s_HsvColorProperty; }
    static winrt::DependencyProperty MaxHueProperty() { return s_MaxHueProperty; }
    static winrt::DependencyProperty MaxSaturationProperty() { return s_MaxSaturationProperty; }
    static winrt::DependencyProperty MaxValueProperty() { return s_MaxValueProperty; }
    static winrt::DependencyProperty MinHueProperty() { return s_MinHueProperty; }
    static winrt::DependencyProperty MinSaturationProperty() { return s_MinSaturationProperty; }
    static winrt::DependencyProperty MinValueProperty() { return s_MinValueProperty; }
    static winrt::DependencyProperty ShapeProperty() { return s_ShapeProperty; }

    static GlobalDependencyProperty s_ColorProperty;
    static GlobalDependencyProperty s_ComponentsProperty;
    static GlobalDependencyProperty s_HsvColorProperty;
    static GlobalDependencyProperty s_MaxHueProperty;
    static GlobalDependencyProperty s_MaxSaturationProperty;
    static GlobalDependencyProperty s_MaxValueProperty;
    static GlobalDependencyProperty s_MinHueProperty;
    static GlobalDependencyProperty s_MinSaturationProperty;
    static GlobalDependencyProperty s_MinValueProperty;
    static GlobalDependencyProperty s_ShapeProperty;

    winrt::event_token ColorChanged(winrt::TypedEventHandler<winrt::ColorSpectrum, winrt::ColorChangedEventArgs> const& value);
    void ColorChanged(winrt::event_token const& token);

    event_source<winrt::TypedEventHandler<winrt::ColorSpectrum, winrt::ColorChangedEventArgs>> m_colorChangedEventSource;

    static void EnsureProperties();
    static void ClearProperties();

    static void OnPropertyChanged(
        winrt::DependencyObject const& sender,
        winrt::DependencyPropertyChangedEventArgs const& args);
};
