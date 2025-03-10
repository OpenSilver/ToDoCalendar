using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace MetroStyleApps
{
    public static class AnimationsHelper
    {
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
            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            animation.BeginTime = TimeSpan.FromSeconds(beginTimeInSeconds);
            EasingDoubleKeyFrame frame = new EasingDoubleKeyFrame();
            frame.Value = targetValue;
            frame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(durationInSeconds));
            if (easingIsEnabled)
            {
                //BackEase easingFunction = new BackEase();
                //easingFunction.Amplitude = 0.3;
                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseOut;
                frame.EasingFunction = easingFunction;
            }
            animation.KeyFrames.Add(frame);
            Storyboard.SetTargetProperty(animation, new PropertyPath(propertyPath));
            Storyboard.SetTarget(animation, targetObject);
            return animation;
        }
    }
}
