﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

#if !BUILD_WINDOWS
using IScrollController = Microsoft.UI.Xaml.Controls.Primitives.IScrollController;
using AnimationMode = Microsoft.UI.Xaml.Controls.AnimationMode;
using SnapPointsMode = Microsoft.UI.Xaml.Controls.SnapPointsMode;
using ScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using ScrollInfo = Microsoft.UI.Xaml.Controls.ScrollInfo;
using ScrollOptions = Microsoft.UI.Xaml.Controls.ScrollOptions;
using ScrollControllerInteractionRequestedEventArgs = Microsoft.UI.Xaml.Controls.Primitives.ScrollControllerInteractionRequestedEventArgs;
using ScrollControllerScrollToRequestedEventArgs = Microsoft.UI.Xaml.Controls.Primitives.ScrollControllerScrollToRequestedEventArgs;
using ScrollControllerScrollByRequestedEventArgs = Microsoft.UI.Xaml.Controls.Primitives.ScrollControllerScrollByRequestedEventArgs;
using ScrollControllerScrollFromRequestedEventArgs = Microsoft.UI.Xaml.Controls.Primitives.ScrollControllerScrollFromRequestedEventArgs;
#endif

namespace MUXControlsTestApp.Utilities
{
    public class CompositionScrollControllerOffsetChangeCompletedEventArgs
    {
        internal CompositionScrollControllerOffsetChangeCompletedEventArgs(int offsetChangeId)
        {
            OffsetChangeId = offsetChangeId;
        }

        public int OffsetChangeId
        {
            get;
            set;
        }
    }

    public sealed class CompositionScrollController : Control, IScrollController
    {
        public event TypedEventHandler<CompositionScrollController, CompositionScrollControllerOffsetChangeCompletedEventArgs> OffsetChangeCompleted;

        public static bool s_IsDebugOutputEnabled = false;

        private const float SmallChangeAdditionalVelocity = 144.0f;
        private const float SmallChangeInertiaDecayRate = 0.975f;

        public enum AnimationType
        {
            Default,
            Accordion,
            Teleportation
        }

        private struct OperationInfo
        {
            public int Id;
            public double RelativeOffsetChange;
            public double OffsetTarget;

            public OperationInfo(int id, double relativeOffsetChange, double offsetTarget) : this()
            {
                Id = id;
                RelativeOffsetChange = relativeOffsetChange;
                OffsetTarget = offsetTarget;
            }
        }

        private List<string> lstAsyncEventMessage = new List<string>();
        private List<int> lstOffsetChangeIds = new List<int>();
        private List<int> lstScrollFromIds = new List<int>();
        private Dictionary<int, OperationInfo> operations = new Dictionary<int, OperationInfo>();
        private FrameworkElement interactionFrameworkElement = null;
        UIElement horizontalGrid = null;
        UIElement verticalGrid = null;
        UIElement horizontalThumb = null;
        UIElement verticalThumb = null;
        RepeatButton horizontalDecrementRepeatButton = null;
        RepeatButton verticalDecrementRepeatButton = null;
        RepeatButton horizontalIncrementRepeatButton = null;
        RepeatButton verticalIncrementRepeatButton = null;
        private Visual interactionVisual = null;
        private Orientation orientation = Orientation.Vertical;
        private ScrollMode scrollMode = ScrollMode.Disabled;
        private bool isThumbDragged = false;
        private bool isThumbPannable = true;
        private bool isThumbPositionMirrored = false;
        private double minOffset = 0.0;
        private double maxOffset = 0.0;
        private double offset = 0.0;
        private double offsetTarget = 0.0;
        private double viewport = 0.0;
        private double preManipulationThumbOffset = 0.0;
        private CompositionPropertySet expressionAnimationSources = null;
        private ExpressionAnimation thumbOffsetAnimation = null;
        private string minOffsetPropertyName;
        private string maxOffsetPropertyName;
        private string offsetPropertyName;
        private string multiplierPropertyName;

        public CompositionScrollController()
        {
            RaiseLogMessage("CompositionScrollController: CompositionScrollController()");

            this.DefaultStyleKey = typeof(CompositionScrollController);
            AreScrollerInteractionsAllowed = true;
            IsAnimatingThumbOffset = true;
            OffsetChangeAnimationType = AnimationType.Default;
            StockOffsetChangeDuration = TimeSpan.MinValue;
            OverriddenOffsetChangeDuration = TimeSpan.MinValue;
            IsEnabledChanged += CompositionScrollController_IsEnabledChanged;
            SizeChanged += CompositionScrollController_SizeChanged;
        }

        protected override void OnApplyTemplate()
        {
            RaiseLogMessage("CompositionScrollController: OnApplyTemplate()");

            UnhookHandlers();

            base.OnApplyTemplate();

            horizontalGrid = GetTemplateChild("HorizontalGrid") as UIElement;
            verticalGrid = GetTemplateChild("VerticalGrid") as UIElement;
            horizontalThumb = GetTemplateChild("HorizontalThumb") as UIElement;
            verticalThumb = GetTemplateChild("VerticalThumb") as UIElement;
            horizontalDecrementRepeatButton = GetTemplateChild("HorizontalDecrementRepeatButton") as RepeatButton;
            verticalDecrementRepeatButton = GetTemplateChild("VerticalDecrementRepeatButton") as RepeatButton;
            horizontalIncrementRepeatButton = GetTemplateChild("HorizontalIncrementRepeatButton") as RepeatButton;
            verticalIncrementRepeatButton = GetTemplateChild("VerticalIncrementRepeatButton") as RepeatButton;

            UpdateOrientation();
        }

        public AnimationType OffsetChangeAnimationType
        {
            get;
            set;
        }

        public TimeSpan StockOffsetChangeDuration
        {
            get;
            private set;
        }

        public TimeSpan OverriddenOffsetChangeDuration
        {
            get;
            set;
        }

        public bool IsAnimatingThumbOffset
        {
            get;
            set;
        }

        public bool IsThumbPannable
        {
            get
            {
                return isThumbPannable;
            }
            set
            {
                RaiseLogMessage("CompositionScrollController: IsThumbPannable(" + value + ")");

                if (isThumbPannable != value)
                {
                    isThumbPannable = value;
                    RaiseInteractionInfoChanged();
                }
            }
        }

        public bool IsThumbPositionMirrored
        {
            get
            {
                return isThumbPositionMirrored;
            }
            set
            {
                RaiseLogMessage("CompositionScrollController: IsThumbPositionMirrored(" + value + ")");

                if (isThumbPositionMirrored != value)
                {
                    if (IsAnimatingThumbOffset)
                    {
                        StopThumbAnimation(Orientation);
                    }

                    isThumbPositionMirrored = value;
                    UpdateInteractionVisualScrollMultiplier();
                    UpdateInteractionFrameworkElementOffset();

                    if (IsAnimatingThumbOffset)
                    {
                        UpdateThumbExpression();
                        StartThumbAnimation(Orientation);
                    }
                }
            }
        }

        public Orientation Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                RaiseLogMessage("CompositionScrollController: Orientation(" + value + ")");

                if (orientation != value)
                {
                    UnhookHandlers();
                    if (IsAnimatingThumbOffset)
                    {
                        StopThumbAnimation(orientation);
                    }
                    orientation = value;
                    UpdateOrientation();
                }
            }
        }

        public void SetExpressionAnimationSources(
            CompositionPropertySet propertySet,
            string minOffsetPropertyName,
            string maxOffsetPropertyName,
            string offsetPropertyName,
            string multiplierPropertyName)
        {
            RaiseLogMessage(
                "CompositionScrollController: SetExpressionAnimationSources for Orientation=" + Orientation +
                " with minOffsetPropertyName=" + minOffsetPropertyName +
                ", maxOffsetPropertyName=" + maxOffsetPropertyName +
                ", offsetPropertyName=" + offsetPropertyName +
                ", multiplierPropertyName=" + multiplierPropertyName);
            expressionAnimationSources = propertySet;
            if (expressionAnimationSources != null)
            {
                this.minOffsetPropertyName = minOffsetPropertyName.Trim();
                this.maxOffsetPropertyName = maxOffsetPropertyName.Trim();
                this.offsetPropertyName = offsetPropertyName.Trim();
                this.multiplierPropertyName = multiplierPropertyName.Trim();

                UpdateInteractionVisualScrollMultiplier();

                if (thumbOffsetAnimation == null && IsAnimatingThumbOffset)
                {
                    EnsureThumbAnimation();
                    UpdateThumbExpression();
                    StartThumbAnimation(Orientation);
                }
            }
            else
            {
                this.minOffsetPropertyName =
                this.maxOffsetPropertyName =
                this.offsetPropertyName =
                this.multiplierPropertyName = string.Empty;

                if (IsAnimatingThumbOffset)
                {
                    thumbOffsetAnimation = null;
                    StopThumbAnimation(Orientation);
                }
            }
        }

        public void SetScrollMode(ScrollMode scrollMode)
        {
            RaiseLogMessage(
                "CompositionScrollController: SetScrollMode for Orientation=" + Orientation +
                " with scrollMode=" + scrollMode);
            this.scrollMode = scrollMode;
            UpdateAreInteractionsAllowed();
        }

        public void SetValues(double minOffset, double maxOffset, double offset, double viewport)
        {
            RaiseLogMessage(
                "CompositionScrollController: SetValues for Orientation=" + Orientation +
                " with minOffset=" + minOffset +
                ", maxOffset=" + maxOffset +
                ", offset=" + offset +
                ", viewport=" + viewport);

            if (maxOffset < minOffset)
            {
                throw new ArgumentOutOfRangeException("maxOffset");
            }

            if (viewport < 0.0)
            {
                throw new ArgumentOutOfRangeException("viewport");
            }

            offset = Math.Max(minOffset, offset);
            offset = Math.Min(maxOffset, offset);

            if (operations.Count == 0)
            {
                offsetTarget = offset;
            }
            else
            {
                offsetTarget = Math.Max(minOffset, offsetTarget);
                offsetTarget = Math.Min(maxOffset, offsetTarget);
            }

            bool updateInteractionFrameworkElementSize =
                this.minOffset != minOffset ||
                this.maxOffset != maxOffset ||
                this.viewport != viewport;

            this.minOffset = minOffset;
            this.offset = offset;
            this.maxOffset = maxOffset;
            this.viewport = viewport;

            if (updateInteractionFrameworkElementSize && !UpdateInteractionFrameworkElementSize())
            {
                UpdateInteractionVisualScrollMultiplier();
            }

            UpdateInteractionFrameworkElementOffset();
        }

        public CompositionAnimation GetScrollAnimation(
            ScrollInfo info,
            Vector2 currentPosition,
            CompositionAnimation defaultAnimation)
        {
            RaiseLogMessage(
                "CompositionScrollController: GetScrollAnimation for Orientation=" + Orientation +
                " with OffsetsChangeId=" + info.OffsetsChangeId + ", currentPosition=" + currentPosition);

            try
            {
                Vector3KeyFrameAnimation stockKeyFrameAnimation = defaultAnimation as Vector3KeyFrameAnimation;

                if (stockKeyFrameAnimation != null)
                {
                    Vector3KeyFrameAnimation customKeyFrameAnimation = stockKeyFrameAnimation;

                    if (OffsetChangeAnimationType != AnimationType.Default)
                    {
                        OperationInfo oi = operations[info.OffsetsChangeId];
                        float positionTarget = (float) oi.OffsetTarget;
                        float otherOrientationPosition = orientation == Orientation.Horizontal ? currentPosition.Y : currentPosition.X;

                        customKeyFrameAnimation = stockKeyFrameAnimation.Compositor.CreateVector3KeyFrameAnimation();
                        if (OffsetChangeAnimationType == AnimationType.Accordion)
                        {
                            float deltaPosition = 0.1f * (float)(positionTarget - offset);

                            if (orientation == Orientation.Horizontal)
                            {
                                for (int step = 0; step < 3; step++)
                                {
                                    customKeyFrameAnimation.InsertKeyFrame(
                                        1.0f - (0.4f / (float)Math.Pow(2, step)),
                                        new Vector3(positionTarget + deltaPosition, otherOrientationPosition, 0.0f));
                                    deltaPosition /= -2.0f;
                                }

                                customKeyFrameAnimation.InsertKeyFrame(1.0f, new Vector3(positionTarget, otherOrientationPosition, 0.0f));
                            }
                            else
                            {
                                for (int step = 0; step < 3; step++)
                                {
                                    customKeyFrameAnimation.InsertKeyFrame(
                                        1.0f - (0.4f / (float)Math.Pow(2, step)),
                                        new Vector3(otherOrientationPosition, positionTarget + deltaPosition, 0.0f));
                                    deltaPosition /= -2.0f;
                                }

                                customKeyFrameAnimation.InsertKeyFrame(1.0f, new Vector3(otherOrientationPosition, positionTarget, 0.0f));
                            }
                        }
                        else
                        {
                            // Teleportation case
                            float deltaPosition = (float)(positionTarget - offset);

                            CubicBezierEasingFunction cubicBezierStart = stockKeyFrameAnimation.Compositor.CreateCubicBezierEasingFunction(
                                new Vector2(1.0f, 0.0f),
                                new Vector2(1.0f, 0.0f));

                            StepEasingFunction step = stockKeyFrameAnimation.Compositor.CreateStepEasingFunction(1);

                            CubicBezierEasingFunction cubicBezierEnd = stockKeyFrameAnimation.Compositor.CreateCubicBezierEasingFunction(
                                new Vector2(0.0f, 1.0f),
                                new Vector2(0.0f, 1.0f));

                            if (orientation == Orientation.Horizontal)
                            {
                                customKeyFrameAnimation.InsertKeyFrame(
                                    0.499999f,
                                    new Vector3(positionTarget - 0.9f * deltaPosition, otherOrientationPosition, 0.0f),
                                    cubicBezierStart);
                                customKeyFrameAnimation.InsertKeyFrame(
                                    0.5f,
                                    new Vector3(positionTarget - 0.1f * deltaPosition, otherOrientationPosition, 0.0f),
                                    step);
                                customKeyFrameAnimation.InsertKeyFrame(
                                    1.0f,
                                    new Vector3(positionTarget, otherOrientationPosition, 0.0f),
                                    cubicBezierEnd);
                            }
                            else
                            {
                                customKeyFrameAnimation.InsertKeyFrame(
                                    0.499999f,
                                    new Vector3(otherOrientationPosition, positionTarget - 0.9f * deltaPosition, 0.0f),
                                    cubicBezierStart);
                                customKeyFrameAnimation.InsertKeyFrame(
                                    0.5f,
                                    new Vector3(otherOrientationPosition, positionTarget - 0.1f * deltaPosition, 0.0f),
                                    step);
                                customKeyFrameAnimation.InsertKeyFrame(
                                    1.0f,
                                    new Vector3(otherOrientationPosition, positionTarget, 0.0f),
                                    cubicBezierEnd);
                            }
                        }
                        customKeyFrameAnimation.Duration = stockKeyFrameAnimation.Duration;
                    }

                    if (OverriddenOffsetChangeDuration != TimeSpan.MinValue)
                    {
                        StockOffsetChangeDuration = stockKeyFrameAnimation.Duration;
                        customKeyFrameAnimation.Duration = OverriddenOffsetChangeDuration;
                    }

                    return customKeyFrameAnimation;
                }
            }
            catch (Exception ex)
            {
                RaiseLogMessage("CompositionScrollController: GetScrollAnimation exception=" + ex);
            }

            return null;
        }

        public void OnScrollCompleted(
            ScrollInfo info)
        {
            int offsetChangeId = info.OffsetsChangeId;

            RaiseLogMessage(
                "CompositionScrollController: OnScrollCompleted for Orientation=" + Orientation +
                " with offsetChangeId=" + offsetChangeId);

            if (lstOffsetChangeIds.Contains(offsetChangeId))
            {
                lstOffsetChangeIds.Remove(offsetChangeId);

                double relativeOffsetChange = 0.0;

                if (operations.ContainsKey(offsetChangeId))
                {
                    OperationInfo oi = operations[offsetChangeId];
                    relativeOffsetChange = oi.RelativeOffsetChange;
                    operations.Remove(offsetChangeId);
                }

                RaiseLogMessage("CompositionScrollController: ScrollTo/By completed. Id=" + offsetChangeId);
            }
            else if (lstScrollFromIds.Contains(offsetChangeId))
            {
                lstScrollFromIds.Add(offsetChangeId);

                RaiseLogMessage("CompositionScrollController: ScrollFromRequest completed. Id=" + offsetChangeId);
            }

            if (OffsetChangeCompleted != null)
            {
                RaiseLogMessage(
                    "CompositionScrollController: OnScrollCompleted raising OffsetChangeCompleted event.");
                OffsetChangeCompleted(this, new CompositionScrollControllerOffsetChangeCompletedEventArgs(offsetChangeId));
            }
        }

        public bool AreInteractionsAllowed
        {
            get;
            private set;
        }


        public bool AreScrollerInteractionsAllowed
        {
            get;
            private set;
        }

        public bool IsInteracting
        {
            get;
            private set;
        }

        public bool IsInteractionVisualRailEnabled
        {
            get
            {
                RaiseLogMessage("CompositionScrollController: get_IsInteractionVisualRailEnabled for Orientation=" + Orientation);
                return true;
            }
        }

        public Visual InteractionVisual
        {
            get
            {
                // Note: Returning interactionVisual causes flickers when interacting with it (InteractionTracker issue).
                RaiseLogMessage("CompositionScrollController: get_InteractionVisual for Orientation=" + Orientation);
                return (IsThumbPannable && IsEnabled && interactionFrameworkElement != null && interactionFrameworkElement.Parent != null) ? 
                    ElementCompositionPreview.GetElementVisual(interactionFrameworkElement.Parent as FrameworkElement) : null;
            }
        }

        public Orientation InteractionVisualScrollOrientation
        {
            get
            {
                RaiseLogMessage("CompositionScrollController: get_InteractionVisualScrollOrientation for Orientation=" + Orientation);
                return Orientation;
            }
        }

        private float InteractionVisualScrollMultiplier
        {
            get
            {
                if (interactionFrameworkElement != null)
                {
                    interactionFrameworkElement.UpdateLayout();

                    double parentDim = 0.0;
                    double interactionFrameworkElementDim =
                        Orientation == Orientation.Horizontal ? interactionFrameworkElement.ActualWidth : interactionFrameworkElement.ActualHeight;
                    FrameworkElement parent = interactionFrameworkElement.Parent as FrameworkElement;
                    if (parent != null)
                    {
                        parentDim = Orientation == Orientation.Horizontal ? parent.ActualWidth : parent.ActualHeight;
                    }
                    if (parentDim != interactionFrameworkElementDim)
                    {
                        RaiseLogMessage("CompositionScrollController: InteractionVisualScrollMultiplier evaluation:");
                        RaiseLogMessage("maxOffset:" + maxOffset);
                        RaiseLogMessage("minOffset:" + minOffset);
                        RaiseLogMessage("interactionFrameworkElementDim:" + interactionFrameworkElementDim);
                        RaiseLogMessage("parentDim:" + parentDim);

                        float multiplier = (float)((maxOffset - minOffset) / (interactionFrameworkElementDim - parentDim));
                        if (IsThumbPositionMirrored)
                            multiplier *= -1.0f;
                        return multiplier;
                    }
                }

                return 0.0f;
            }
        }

        public event TypedEventHandler<CompositionScrollController, string> LogMessage;

        public event TypedEventHandler<IScrollController, Object> InteractionInfoChanged;
        public event TypedEventHandler<IScrollController, ScrollControllerInteractionRequestedEventArgs> InteractionRequested;
        public event TypedEventHandler<IScrollController, ScrollControllerScrollToRequestedEventArgs> ScrollToRequested;
        public event TypedEventHandler<IScrollController, ScrollControllerScrollByRequestedEventArgs> ScrollByRequested;
        public event TypedEventHandler<IScrollController, ScrollControllerScrollFromRequestedEventArgs> ScrollFromRequested;

        public int ScrollTo(
            double offset, AnimationMode animationMode)
        {
            RaiseLogMessage("CompositionScrollController: ScrollTo for Orientation=" + Orientation + " with offset=" + offset + ", animationMode=" + animationMode);

            return RaiseScrollToRequested(
                offset, animationMode, false /*hookupCompletion*/);
        }

        public int ScrollBy(
            double offsetDelta, AnimationMode animationMode)
        {
            RaiseLogMessage("CompositionScrollController: ScrollBy for Orientation=" + Orientation + " with offsetDelta=" + offsetDelta + ", animationMode=" + animationMode);

            return RaiseScrollByRequested(
                offsetDelta, animationMode, false /*hookupCompletion*/);
        }

        public int ScrollFrom(
            float offsetVelocity, float? inertiaDecayRate)
        {
            RaiseLogMessage("CompositionScrollController: ScrollFrom for Orientation=" + Orientation + " with offsetVelocity=" + offsetVelocity + ", inertiaDecayRate=" + inertiaDecayRate);

            return RaiseScrollFromRequested(
                offsetVelocity, inertiaDecayRate, false /*hookupCompletion*/);
        }

        private void RaiseLogMessage(string message)
        {
            if (s_IsDebugOutputEnabled)
                System.Diagnostics.Debug.WriteLine(message);

            if (LogMessage != null)
            {
                LogMessage(this, message);
            }
        }

        private void RaiseInteractionInfoChanged()
        {
            RaiseLogMessage("CompositionScrollController: RaiseInteractionInfoChanged for Orientation=" + Orientation);
            if (InteractionInfoChanged != null)
            {
                InteractionInfoChanged(this, null);
            }
        }

        private void RaiseInteractionRequested(PointerPoint pointerPoint)
        {
            RaiseLogMessage("CompositionScrollController: RaiseInteractionRequested for Orientation=" + Orientation + " with pointerPoint=" + pointerPoint);
            if (InteractionRequested != null)
            {
                ScrollControllerInteractionRequestedEventArgs args = new ScrollControllerInteractionRequestedEventArgs(pointerPoint);
                InteractionRequested(this, args);
                RaiseLogMessage("CompositionScrollController: RaiseInteractionRequested result Handled=" + args.Handled);
            }
        }

        private int RaiseScrollToRequested(
            double offset,
            AnimationMode animationMode,
            bool hookupCompletion)
        {
            RaiseLogMessage("CompositionScrollController: RaiseScrollToRequested for Orientation=" + Orientation + " with offset=" + offset + ", animationMode=" + animationMode);

            try
            {
                if (ScrollToRequested != null)
                {
                    ScrollControllerScrollToRequestedEventArgs e =
                        new ScrollControllerScrollToRequestedEventArgs(
                            offset,
                            new ScrollOptions(animationMode, SnapPointsMode.Ignore));
                    ScrollToRequested(this, e);
                    if (e.Info.OffsetsChangeId != -1)
                    {
                        RaiseLogMessage("CompositionScrollController: ScrollToRequest started. OffsetsChangeId=" + e.Info.OffsetsChangeId);

                        if (hookupCompletion && !lstOffsetChangeIds.Contains(e.Info.OffsetsChangeId))
                        {
                            lstOffsetChangeIds.Add(e.Info.OffsetsChangeId);
                        }
                    }
                    return e.Info.OffsetsChangeId;
                }
            }
            catch (Exception ex)
            {
                RaiseLogMessage("CompositionScrollController: RaiseScrollToRequested - exception=" + ex);
            }

            return -1;
        }

        private int RaiseScrollByRequested(
            double offsetDelta,
            AnimationMode animationMode,
            bool hookupCompletion)
        {
            RaiseLogMessage("CompositionScrollController: RaiseScrollByRequested for Orientation=" + Orientation + " with offsetDelta=" + offsetDelta + ", animationMode=" + animationMode);

            try
            {
                if (ScrollByRequested != null)
                {
                    ScrollControllerScrollByRequestedEventArgs e =
                        new ScrollControllerScrollByRequestedEventArgs(
                            offsetDelta,
                            new ScrollOptions(animationMode, SnapPointsMode.Ignore));
                    ScrollByRequested(this, e);
                    if (e.Info.OffsetsChangeId != -1)
                    {
                        RaiseLogMessage("CompositionScrollController: ScrollByRequest started. OffsetsChangeId=" + e.Info.OffsetsChangeId);

                        if (hookupCompletion && !lstOffsetChangeIds.Contains(e.Info.OffsetsChangeId))
                        {
                            lstOffsetChangeIds.Add(e.Info.OffsetsChangeId);
                        }
                    }
                    return e.Info.OffsetsChangeId;
                }
            }
            catch (Exception ex)
            {
                RaiseLogMessage("CompositionScrollController: RaiseScrollByRequested - exception=" + ex);
            }

            return -1;
        }

        private int RaiseScrollFromRequested(
            float offsetVelocity, float? inertiaDecayRate, bool hookupCompletion)
        {
            RaiseLogMessage("CompositionScrollController: RaiseScrollFromRequested for Orientation=" + Orientation + " with offsetVelocity=" + offsetVelocity + ", inertiaDecayRate=" + inertiaDecayRate);

            if (ScrollFromRequested != null)
            {
                ScrollControllerScrollFromRequestedEventArgs e =
                    new ScrollControllerScrollFromRequestedEventArgs(
                        offsetVelocity,
                        inertiaDecayRate);
                ScrollFromRequested(this, e);
                if (e.Info.OffsetsChangeId != -1)
                {
                    RaiseLogMessage("CompositionScrollController: ScrollFromRequest started. OffsetsChangeId=" + e.Info.OffsetsChangeId);

                    if (hookupCompletion && !lstScrollFromIds.Contains(e.Info.OffsetsChangeId))
                    {
                        lstScrollFromIds.Add(e.Info.OffsetsChangeId);
                    }
                }
                return e.Info.OffsetsChangeId;
            }
            return -1;
        }

        private void InteractionFrameworkElement_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Point pt = e.GetCurrentPoint(sender as UIElement).Position;
            RaiseLogMessage("CompositionScrollController: InteractionFrameworkElement_PointerPressed for Orientation=" + Orientation + ", position=" + pt);

            switch (e.Pointer.PointerDeviceType)
            {
                case Windows.Devices.Input.PointerDeviceType.Touch:
                case Windows.Devices.Input.PointerDeviceType.Pen:
                    RaiseInteractionRequested(e.GetCurrentPoint(null));
                    break;
                case Windows.Devices.Input.PointerDeviceType.Mouse:
                    isThumbDragged = true;
                    AreScrollerInteractionsAllowed = false;
                    break;
            }
        }

        private void InteractionFrameworkElement_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            if (isThumbDragged)
            {
                preManipulationThumbOffset = Orientation == Orientation.Horizontal ? HorizontalThumbOffset : VerticalThumbOffset;
            }
        }

        private void InteractionFrameworkElement_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (isThumbDragged)
            {
                double targetThumbOffset = preManipulationThumbOffset + (Orientation == Orientation.Horizontal ? e.Cumulative.Translation.X : e.Cumulative.Translation.Y);
                double scrollerOffset = ScrollerOffsetFromThumbOffset(targetThumbOffset);

                int offsetChangeId = RaiseScrollToRequested(
                    scrollerOffset, AnimationMode.Disabled, true /*hookupCompletion*/);
            }
        }

        private void InteractionFrameworkElement_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (isThumbDragged)
            {
                isThumbDragged = false;
                AreScrollerInteractionsAllowed = true;
            }
        }

        private void UnhookHandlers()
        {
            if (horizontalDecrementRepeatButton != null)
            {
                horizontalDecrementRepeatButton.Click -= DecrementRepeatButton_Click;
            }

            if (horizontalIncrementRepeatButton != null)
            {
                horizontalIncrementRepeatButton.Click -= IncrementRepeatButton_Click;
            }

            if (verticalDecrementRepeatButton != null)
            {
                verticalDecrementRepeatButton.Click -= DecrementRepeatButton_Click;
            }

            if (verticalIncrementRepeatButton != null)
            {
                verticalIncrementRepeatButton.Click -= IncrementRepeatButton_Click;
            }

            if (interactionFrameworkElement != null)
            {
                interactionFrameworkElement.PointerPressed -= InteractionFrameworkElement_PointerPressed;
                interactionFrameworkElement.ManipulationStarting -= InteractionFrameworkElement_ManipulationStarting;
                interactionFrameworkElement.ManipulationDelta -= InteractionFrameworkElement_ManipulationDelta;
                interactionFrameworkElement.ManipulationCompleted -= InteractionFrameworkElement_ManipulationCompleted;

                FrameworkElement parent = interactionFrameworkElement.Parent as FrameworkElement;
                if (parent != null)
                {
                    parent.PointerPressed -= Parent_PointerPressed;
                }
            }
        }

        private bool UpdateAreInteractionsAllowed()
        {
            bool oldAreInteractionsAllowed = AreInteractionsAllowed;

            AreInteractionsAllowed = scrollMode != ScrollMode.Disabled && IsEnabled;

            if (oldAreInteractionsAllowed != AreInteractionsAllowed)
            {
                RaiseInteractionInfoChanged();
                return true;
            }
            return false;
        }

        private void UpdateOrientation()
        {
            if (Orientation == Orientation.Horizontal)
            {
                if (horizontalGrid != null)
                    horizontalGrid.Visibility = Visibility.Visible;
                if (verticalGrid != null)
                    verticalGrid.Visibility = Visibility.Collapsed;
                interactionFrameworkElement = horizontalThumb as FrameworkElement;
                if (interactionFrameworkElement != null)
                {
                    interactionFrameworkElement.ManipulationMode = ManipulationModes.TranslateX;
                }

                if (horizontalDecrementRepeatButton != null)
                {
                    horizontalDecrementRepeatButton.Click += DecrementRepeatButton_Click;
                }

                if (horizontalIncrementRepeatButton != null)
                {
                    horizontalIncrementRepeatButton.Click += IncrementRepeatButton_Click;
                }
            }
            else
            {
                if (verticalGrid != null)
                    verticalGrid.Visibility = Visibility.Visible;
                if (horizontalGrid != null)
                    horizontalGrid.Visibility = Visibility.Collapsed;
                interactionFrameworkElement = verticalThumb as FrameworkElement;
                if (interactionFrameworkElement != null)
                {
                    interactionFrameworkElement.ManipulationMode = ManipulationModes.TranslateY;
                }

                if (verticalDecrementRepeatButton != null)
                {
                    verticalDecrementRepeatButton.Click += DecrementRepeatButton_Click;
                }

                if (verticalIncrementRepeatButton != null)
                {
                    verticalIncrementRepeatButton.Click += IncrementRepeatButton_Click;
                }
            }

            if (interactionFrameworkElement != null)
            {
                interactionVisual = ElementCompositionPreview.GetElementVisual(interactionFrameworkElement);
                ElementCompositionPreview.SetIsTranslationEnabled(interactionFrameworkElement, true);

                if (IsAnimatingThumbOffset)
                {
                    StartThumbAnimation(Orientation);
                }

                interactionFrameworkElement.PointerPressed += InteractionFrameworkElement_PointerPressed;
                interactionFrameworkElement.ManipulationStarting += InteractionFrameworkElement_ManipulationStarting;
                interactionFrameworkElement.ManipulationDelta += InteractionFrameworkElement_ManipulationDelta;
                interactionFrameworkElement.ManipulationCompleted += InteractionFrameworkElement_ManipulationCompleted;

                FrameworkElement parent = interactionFrameworkElement.Parent as FrameworkElement;
                if (parent != null)
                {
                    parent.IsTapEnabled = true;
                    parent.PointerPressed += Parent_PointerPressed;
                }
            }
            else
            {
                interactionVisual = null;
            }

            RaiseInteractionInfoChanged();
        }

        private void UpdateInteractionVisualScrollMultiplier()
        {
            if (expressionAnimationSources != null && !string.IsNullOrWhiteSpace(multiplierPropertyName))
            {
                float interactionVisualScrollMultiplier = InteractionVisualScrollMultiplier;

                RaiseLogMessage("CompositionScrollController: UpdateInteractionVisualScrollMultiplier for Orientation=" + Orientation + ", InteractionVisualScrollMultiplier=" + interactionVisualScrollMultiplier);
                expressionAnimationSources.InsertScalar(multiplierPropertyName, interactionVisualScrollMultiplier);
            }
        }

        private void UpdateInteractionFrameworkElementOffset()
        {
            if (!IsAnimatingThumbOffset && interactionVisual != null)
            {
                if (orientation == Orientation.Horizontal)
                {
                    float horizontalThumbOffset = (float)HorizontalThumbOffset;
                    Vector3 currentHorizontalThumbOffset;

                    interactionVisual.Properties.TryGetVector3("Translation", out currentHorizontalThumbOffset);
                    if (currentHorizontalThumbOffset.X != horizontalThumbOffset)
                    {
                        RaiseLogMessage("CompositionScrollController: UpdateInteractionFrameworkElementOffset for Orientation=Horizontal, HorizontalThumbOffset=" + horizontalThumbOffset);
                        interactionVisual.Properties.InsertVector3("Translation", new Vector3(horizontalThumbOffset, 0.0f, 0.0f));
                    }
                }
                else
                {
                    float verticalThumbOffset = (float)VerticalThumbOffset;
                    Vector3 currentVerticalThumbOffset;

                    interactionVisual.Properties.TryGetVector3("Translation", out currentVerticalThumbOffset);
                    if (currentVerticalThumbOffset.Y != verticalThumbOffset)
                    {
                        RaiseLogMessage("CompositionScrollController: UpdateInteractionFrameworkElementOffset for Orientation=Vertical, VerticalThumbOffset=" + verticalThumbOffset);
                        interactionVisual.Properties.InsertVector3("Translation", new Vector3(0.0f, verticalThumbOffset, 0.0f));
                    }
                }
            }
        }

        private bool UpdateInteractionFrameworkElementSize()
        {
            if (interactionFrameworkElement != null)
            {
                double parentWidth = 0.0;
                double parentHeight = 0.0;
                FrameworkElement parent = interactionFrameworkElement.Parent as FrameworkElement;

                if (parent != null)
                {
                    parentWidth = parent.ActualWidth;
                    parentHeight = parent.ActualHeight;
                }

                if (orientation == Orientation.Horizontal)
                {
                    double newWidth;
                    if (viewport == 0.0)
                    {
                        newWidth = 40.0;
                    }
                    else
                    {
                        newWidth = Math.Max(Math.Min(40.0, parentWidth), viewport / (maxOffset - minOffset + viewport) * parentWidth);
                    }
                    if (newWidth != interactionFrameworkElement.Width)
                    {
                        RaiseLogMessage("CompositionScrollController: UpdateInteractionFrameworkElementSize for Orientation=Horizontal, setting Width=" + newWidth);
                        interactionFrameworkElement.Width = newWidth;
                        var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, UpdateInteractionVisualScrollMultiplier);
                        return true;
                    }
                }
                else
                {
                    double newHeight;
                    if (viewport == 0.0)
                    {
                        newHeight = 40.0;
                    }
                    else
                    {
                        newHeight = Math.Max(Math.Min(40.0, parentHeight), viewport / (maxOffset - minOffset + viewport) * parentHeight);
                    }
                    if (newHeight != interactionFrameworkElement.Height)
                    {
                        RaiseLogMessage("CompositionScrollController: UpdateInteractionFrameworkElementSize for Orientation=Vertical, setting Height=" + newHeight);
                        interactionFrameworkElement.Height = newHeight;
                        var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, UpdateInteractionVisualScrollMultiplier);
                        return true;
                    }
                }
            }
            return false;
        }

        private void EnsureThumbAnimation()
        {
            if (thumbOffsetAnimation == null && expressionAnimationSources != null &&
                !string.IsNullOrWhiteSpace(multiplierPropertyName) &&
                !string.IsNullOrWhiteSpace(offsetPropertyName) &&
                !string.IsNullOrWhiteSpace(minOffsetPropertyName) &&
                !string.IsNullOrWhiteSpace(maxOffsetPropertyName))
            { 
                thumbOffsetAnimation = expressionAnimationSources.Compositor.CreateExpressionAnimation();
                thumbOffsetAnimation.SetReferenceParameter("eas", expressionAnimationSources);
            }
        }

        private void UpdateThumbExpression()
        {
            if (thumbOffsetAnimation != null)
            {
                if (IsThumbPositionMirrored)
                {
                    thumbOffsetAnimation.Expression =
                        "(eas." + maxOffsetPropertyName + " - min(eas." + maxOffsetPropertyName + ",max(eas." + minOffsetPropertyName + ",eas." + offsetPropertyName + ")))/(eas." + multiplierPropertyName + ")";
                }
                else
                {
                    thumbOffsetAnimation.Expression =
                        "min(eas." + maxOffsetPropertyName + ",max(eas." + minOffsetPropertyName + ",eas." + offsetPropertyName + "))/(-eas." + multiplierPropertyName + ")";
                }
            }
        }

        private void StartThumbAnimation(Orientation orientation)
        {
            if (interactionVisual != null && thumbOffsetAnimation != null)
            {
                if (orientation == Orientation.Horizontal)
                {
                    interactionVisual.StartAnimation("Translation.X", thumbOffsetAnimation);
                }
                else
                {
                    interactionVisual.StartAnimation("Translation.Y", thumbOffsetAnimation);
                }
            }
        }

        private void StopThumbAnimation(Orientation orientation)
        {
            if (interactionVisual != null)
            {
                if (orientation == Orientation.Horizontal)
                {
                    interactionVisual.StopAnimation("Translation.X");
                }
                else
                {
                    interactionVisual.StopAnimation("Translation.Y");
                }
            }
        }

        private void DecrementRepeatButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseLogMessage("CompositionScrollController: DecrementRepeatButton_Click for Orientation=" + Orientation);

            int offsetChangeId =
                RaiseScrollFromRequested(IsThumbPositionMirrored ? SmallChangeAdditionalVelocity : - SmallChangeAdditionalVelocity, SmallChangeInertiaDecayRate, true /*hookupCompletion*/);
        }

        private void IncrementRepeatButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseLogMessage("CompositionScrollController: IncrementRepeatButton_Click for Orientation=" + Orientation);

            int offsetChangeId =
                RaiseScrollFromRequested(IsThumbPositionMirrored ? -SmallChangeAdditionalVelocity : SmallChangeAdditionalVelocity, SmallChangeInertiaDecayRate, true /*hookupCompletion*/);
        }

        private double HorizontalThumbOffset
        {
            get
            {
                if (interactionFrameworkElement != null)
                {
                    double parentWidth = 0.0;
                    FrameworkElement parent = interactionFrameworkElement.Parent as FrameworkElement;
                    if (parent != null)
                    {
                        parentWidth = parent.ActualWidth;
                    }
                    if (maxOffset != minOffset)
                    {
                        if (IsThumbPositionMirrored)
                        {
                            return (maxOffset - offset) / (maxOffset - minOffset) * (parentWidth - interactionFrameworkElement.Width);
                        }
                        else
                        {
                            return (offset - minOffset) / (maxOffset - minOffset) * (parentWidth - interactionFrameworkElement.Width);
                        }
                    }
                }

                return 0.0;
            }
        }

        private double VerticalThumbOffset
        {
            get
            {
                if (interactionFrameworkElement != null)
                {
                    double parentHeight = 0.0;
                    FrameworkElement parent = interactionFrameworkElement.Parent as FrameworkElement;
                    if (parent != null)
                    {
                        parentHeight = parent.ActualHeight;
                    }
                    if (maxOffset != minOffset)
                    {
                        if (IsThumbPositionMirrored)
                        {
                            return (maxOffset - offset) / (maxOffset - minOffset) * (parentHeight - interactionFrameworkElement.Height);
                        }
                        else
                        {
                            return (offset - minOffset) / (maxOffset - minOffset) * (parentHeight - interactionFrameworkElement.Height);
                        }
                    }
                }

                return 0.0;
            }
        }

        private double ScrollerOffsetFromThumbOffset(double thumbOffset)
        {
            double scrollerOffset = 0.0;

            if (interactionFrameworkElement != null)
            {
                double parentDim = 0.0;
                double interactionFrameworkElementDim = 
                    Orientation == Orientation.Horizontal ? interactionFrameworkElement.ActualWidth : interactionFrameworkElement.ActualHeight;
                FrameworkElement parent = interactionFrameworkElement.Parent as FrameworkElement;
                if (parent != null)
                {
                    parentDim = Orientation == Orientation.Horizontal ? parent.ActualWidth : parent.ActualHeight;
                }
                if (parentDim != interactionFrameworkElementDim)
                {
                    if (IsThumbPositionMirrored)
                    {
                        scrollerOffset = (parentDim - interactionFrameworkElementDim - thumbOffset) * (maxOffset - minOffset) / (parentDim - interactionFrameworkElementDim);
                    }
                    else
                    {
                        scrollerOffset = thumbOffset * (maxOffset - minOffset) / (parentDim - interactionFrameworkElementDim);
                    }
                }
            }

            return scrollerOffset;
        }

        private void CompositionScrollController_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateInteractionFrameworkElementSize();
            UpdateInteractionFrameworkElementOffset();
        }

        private void CompositionScrollController_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RaiseLogMessage("CompositionScrollController: IsEnabledChanged for Orientation=" + Orientation + ", IsEnabled=" + IsEnabled);
            if (!UpdateAreInteractionsAllowed())
            {
                RaiseInteractionInfoChanged();
            }
        }

        private void Parent_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Point pt = e.GetCurrentPoint(sender as UIElement).Position;
            RaiseLogMessage("CompositionScrollController: Parent_PointerPressed for Orientation=" + Orientation + ", position=" + pt);
            double relativeOffsetChange = 0.0;
            double newOffsetTarget = 0.0;

            if (Orientation == Orientation.Horizontal)
            {
                if (pt.X < HorizontalThumbOffset)
                {
                    if (IsThumbPositionMirrored)
                        relativeOffsetChange = viewport;
                    else
                        relativeOffsetChange = -viewport;
                }
                else if (pt.X > HorizontalThumbOffset + interactionFrameworkElement.ActualWidth)
                {
                    if (IsThumbPositionMirrored)
                        relativeOffsetChange = -viewport;
                    else
                        relativeOffsetChange = viewport;
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (pt.Y < VerticalThumbOffset)
                {
                    if (IsThumbPositionMirrored)
                        relativeOffsetChange = viewport;
                    else
                        relativeOffsetChange = -viewport;
                }
                else if (pt.Y > VerticalThumbOffset + interactionFrameworkElement.ActualHeight)
                {
                    if (IsThumbPositionMirrored)
                        relativeOffsetChange = -viewport;
                    else
                        relativeOffsetChange = viewport;
                }
                else
                {
                    return;
                }
            }

            newOffsetTarget = offsetTarget + relativeOffsetChange;
            newOffsetTarget = Math.Max(minOffset, newOffsetTarget);
            newOffsetTarget = Math.Min(maxOffset, newOffsetTarget);
            relativeOffsetChange = newOffsetTarget - offsetTarget;
            offsetTarget = newOffsetTarget;

            int offsetChangeId = RaiseScrollToRequested(
                offsetTarget, AnimationMode.Auto, true /*hookupCompletion*/);
            if (offsetChangeId != -1 && !operations.ContainsKey(offsetChangeId))
            {
                operations.Add(offsetChangeId, new OperationInfo(offsetChangeId, relativeOffsetChange, offsetTarget));
            }
        }
    }
}
