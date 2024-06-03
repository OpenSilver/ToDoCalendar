#if SILVERLIGHT
using System;
using System.Windows;
using System.Windows.Controls;
#elif WINRT
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace MetroStyleApps
{
    public partial class DragAndDropTarget : Control
    {
        public DragAndDropTarget()
        {
            this.Loaded += DragAndDropTarget_Loaded;
            this.Unloaded += DragAndDropTarget_Unloaded;
        }


        public object GroupId { get; set; } // The "GroupId" is used in a similar way to the "GroupName" of a RadioButton: that is, to couple the drag and drop controls with the corresponding targets:
        public event EventHandler<DragAndDropEventArgs> SourceDropped;

        DragAndDropRoot _dragAndDropRoot;

        void DragAndDropTarget_Loaded(object sender, RoutedEventArgs e)
        {
            // Register this control so that when the user initiates the drap & drop operation, the system knows where the targets are:
            _dragAndDropRoot = MetroHelpers.GetParentOfType<DragAndDropRoot>(this);
            if (_dragAndDropRoot != null)
                _dragAndDropRoot.RegisterDragAndDropTarget(this);
        }

        void DragAndDropTarget_Unloaded(object sender, RoutedEventArgs e)
        {
            // Unregister this control:
            if (_dragAndDropRoot != null)
                _dragAndDropRoot.UnregisterDragAndDropTarget(this);
        }

        bool _isHighlightedForSignalingSelectedTarget;
        public bool IsHighlightedForSignalingSelectedTarget
        {
            get { return _isHighlightedForSignalingSelectedTarget; }
        }

        public void ShowHighlightingForSignalingSelectedTarget(double sourceWidth, double sourceHeight)
        {
            if (!_isHighlightedForSignalingSelectedTarget)
            {
                AnimationsHelper.SetPropertyWithAnimation(this, "(FrameworkElement.MinWidth)", sourceWidth, DragAndDropRoot.DefaultDurationOfAnimations);
                AnimationsHelper.SetPropertyWithAnimation(this, "(FrameworkElement.MinHeight)", sourceHeight, DragAndDropRoot.DefaultDurationOfAnimations);
                _isHighlightedForSignalingSelectedTarget = true;
            }
        }

        public void HideHighlightingForSignalingSelectedTarget(bool instantly = false)
        {
            if (_isHighlightedForSignalingSelectedTarget)
            {
                AnimationsHelper.SetPropertyWithAnimation(this, "(FrameworkElement.MinWidth)", 0d, (instantly ? 0d : DragAndDropRoot.DefaultDurationOfAnimations));
                AnimationsHelper.SetPropertyWithAnimation(this, "(FrameworkElement.MinHeight)", 0d, (instantly ? 0d : DragAndDropRoot.DefaultDurationOfAnimations));
                // Note: when the "instantly" value is True, we could have set the properties directly without using an animation. However, using an animation (with a duration of 0 seconds) has the advantage that it stops all the other animations that are pending on the same property.

                _isHighlightedForSignalingSelectedTarget = false;
            }
        }

        bool _isHighlightedForSignalingAvailableTargets;
        public bool IsHighlightedForSignalingAvailableTargets
        {
            get { return _isHighlightedForSignalingAvailableTargets; }
            set
            {
                if (value != _isHighlightedForSignalingAvailableTargets)
                {
                    _isHighlightedForSignalingAvailableTargets = value;
                    //if (value)
                    //    LayoutRoot.Background = new SolidColorBrush(Colors.Red);
                    //else
                    //    LayoutRoot.Background = new SolidColorBrush(Colors.White);
                }
            }
        }

        public void OnSourceDropped(DragAndDropEventArgs args)
        {
            if (SourceDropped != null)
                SourceDropped(this, args);
        }
    }
}
