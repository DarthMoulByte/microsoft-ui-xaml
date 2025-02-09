﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#pragma once
#include "CornerRadiusFilterConverter.g.h"

class CornerRadiusFilterConverter :
    public winrt::implementation::CornerRadiusFilterConverterT<CornerRadiusFilterConverter>
{
public:
    enum class FilterType
    {
        Top,
        Right,
        Left,
        Bottom
    };

    CornerRadiusFilterConverter();

    winrt::CornerRadius Convert(
        winrt::CornerRadius const& radius,
        FilterType const& filter);

    winrt::IInspectable Convert(
        winrt::IInspectable const& value,
        winrt::TypeName const& targetType,
        winrt::IInspectable const& parameter,
        winrt::hstring const& language);

    winrt::IInspectable ConvertBack(
        winrt::IInspectable const& value,
        winrt::TypeName const& targetType,
        winrt::IInspectable const& parameter,
        winrt::hstring const& language);
};
