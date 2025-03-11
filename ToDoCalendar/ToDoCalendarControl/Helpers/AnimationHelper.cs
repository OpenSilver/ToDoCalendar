using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

public static class AnimationHelper
{
    public static void ShowWithAnimation(UIElement element)
    {
        // Apply RenderTransform if not already set
        var scaleTransform = new ScaleTransform { ScaleX = 0, ScaleY = 0 };
        element.RenderTransform = scaleTransform;
        element.RenderTransformOrigin = new Point(0, 1);

        // Ensure visibility before animation
        element.Visibility = Visibility.Visible;

        // Create the scale animation for X
        var scaleXAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new QuadraticEase
            {
                EasingMode = EasingMode.EaseOut
            }
        };

        // Create a separate scale animation for Y
        var scaleYAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new QuadraticEase
            {
                EasingMode = EasingMode.EaseOut
            }
        };

        // Create storyboard
        var storyboard = new Storyboard();

        Storyboard.SetTarget(scaleXAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

        Storyboard.SetTarget(scaleYAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

        storyboard.Children.Add(scaleXAnimation);
        storyboard.Children.Add(scaleYAnimation);

        storyboard.Begin();
    }

    public static void HideWithAnimation(UIElement element)
    {
        if (element.Visibility != Visibility.Visible)
            return;

        var scaleTransform = element.RenderTransform as ScaleTransform ?? new ScaleTransform { ScaleX = 1, ScaleY = 1 };
        element.RenderTransform = scaleTransform;
        element.RenderTransformOrigin = new Point(0, 1);

        var scaleXAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        var scaleYAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        var storyboard = new Storyboard();
        Storyboard.SetTarget(scaleXAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

        Storyboard.SetTarget(scaleYAnimation, scaleTransform);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

        storyboard.Children.Add(scaleXAnimation);
        storyboard.Children.Add(scaleYAnimation);

        // Set visibility to Collapsed after animation completes
        storyboard.Completed += (s, e) => element.Visibility = Visibility.Collapsed;

        storyboard.Begin();
    }

    public static void SetPropertyWithAnimation(DependencyObject targetObject, string propertyPath, double targetValue, double durationInSeconds, Action actionWhenCompleted = null)
    {
        var storyboard = new Storyboard();
        storyboard.Children.Add(CreateAnimation(durationInSeconds, targetValue, targetObject, propertyPath, true));
        if (actionWhenCompleted != null)
        {
            storyboard.Completed += (s, e) =>
            {
                actionWhenCompleted();
            };
        }
        storyboard.Begin();
    }

    public static DoubleAnimationUsingKeyFrames CreateAnimation(double durationInSeconds, double targetValue, DependencyObject targetObject, string propertyPath, bool easingIsEnabled, double beginTimeInSeconds = 0)
    {
        DoubleAnimationUsingKeyFrames animation = new()
        {
            BeginTime = TimeSpan.FromSeconds(beginTimeInSeconds)
        };
        EasingDoubleKeyFrame frame = new()
        {
            Value = targetValue,
            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(durationInSeconds))
        };
        if (easingIsEnabled)
        {
            //BackEase easingFunction = new BackEase();
            //easingFunction.Amplitude = 0.3;
            ExponentialEase easingFunction = new()
            {
                EasingMode = EasingMode.EaseOut
            };
            frame.EasingFunction = easingFunction;
        }
        animation.KeyFrames.Add(frame);
        Storyboard.SetTargetProperty(animation, new PropertyPath(propertyPath));
        Storyboard.SetTarget(animation, targetObject);
        return animation;
    }
}
