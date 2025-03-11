using System;
using System.Windows;
using System.Windows.Controls;
using ToDoCalendarControl.Helpers;

namespace ToDoCalendarControl.Controls
{
    public class DragAndDropTarget : Control
    {
        public DragAndDropTarget()
        {
            Loaded += DragAndDropTarget_Loaded;
            Unloaded += DragAndDropTarget_Unloaded;
        }

        public object GroupId { get; set; } // The "GroupId" is used in a similar way to the "GroupName" of a RadioButton: that is, to couple the drag and drop controls with the corresponding targets:
        public event EventHandler<DragAndDropEventArgs> SourceDropped;

        private DragAndDropRoot _dragAndDropRoot;

        private void DragAndDropTarget_Loaded(object sender, RoutedEventArgs e)
        {
            // Register this control so that when the user initiates the drap & drop operation, the system knows where the targets are:
            _dragAndDropRoot = UIHelper.GetParentOfType<DragAndDropRoot>(this);
            _dragAndDropRoot?.RegisterDragAndDropTarget(this);
        }

        private void DragAndDropTarget_Unloaded(object sender, RoutedEventArgs e)
        {
            // Unregister this control:
            _dragAndDropRoot?.UnregisterDragAndDropTarget(this);
        }

        private bool _isHighlightedForSignalingSelectedTarget;
        public bool IsHighlightedForSignalingSelectedTarget => _isHighlightedForSignalingSelectedTarget;

        public void ShowHighlightingForSignalingSelectedTarget(double sourceWidth, double sourceHeight)
        {
            if (!_isHighlightedForSignalingSelectedTarget)
            {
                AnimationHelper.SetPropertyWithAnimation(this, "(FrameworkElement.MinWidth)", sourceWidth, DragAndDropRoot.DefaultDurationOfAnimations);
                AnimationHelper.SetPropertyWithAnimation(this, "(FrameworkElement.MinHeight)", sourceHeight, DragAndDropRoot.DefaultDurationOfAnimations);
                _isHighlightedForSignalingSelectedTarget = true;
            }
        }

        public void HideHighlightingForSignalingSelectedTarget(bool instantly = false)
        {
            if (_isHighlightedForSignalingSelectedTarget)
            {
                AnimationHelper.SetPropertyWithAnimation(this, "(FrameworkElement.MinWidth)", 0d, instantly ? 0d : DragAndDropRoot.DefaultDurationOfAnimations);
                AnimationHelper.SetPropertyWithAnimation(this, "(FrameworkElement.MinHeight)", 0d, instantly ? 0d : DragAndDropRoot.DefaultDurationOfAnimations);
                // Note: when the "instantly" value is True, we could have set the properties directly without using an animation. However, using an animation (with a duration of 0 seconds) has the advantage that it stops all the other animations that are pending on the same property.

                _isHighlightedForSignalingSelectedTarget = false;
            }
        }

        private bool _isHighlightedForSignalingAvailableTargets;
        public bool IsHighlightedForSignalingAvailableTargets
        {
            get => _isHighlightedForSignalingAvailableTargets;
            set
            {
                if (value != _isHighlightedForSignalingAvailableTargets)
                {
                    _isHighlightedForSignalingAvailableTargets = value;
                }
            }
        }

        public void OnSourceDropped(DragAndDropEventArgs args)
        {
            SourceDropped?.Invoke(this, args);
        }
    }
}
