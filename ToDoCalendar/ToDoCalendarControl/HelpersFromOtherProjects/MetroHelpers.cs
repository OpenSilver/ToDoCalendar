using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WinRTForSilverlight;

namespace MetroStyleApps
{
    public class MetroHelpers
    {
        public static UIElement GetRootVisual()
        {
            return Application.Current.RootVisual;
        }

        public static FrameworkElement GetParentRoot(FrameworkElement parent)
        {
            FrameworkElement parentRoot = parent;
            FrameworkElement lastParentRoot = parentRoot;
            while (lastParentRoot != null)
            {
                lastParentRoot = VisualTreeHelper.GetParent(lastParentRoot) as FrameworkElement;
                if (lastParentRoot != null)
                    parentRoot = lastParentRoot;
            }
            return parentRoot;
        }

        public static T GetParentOfType<T>(DependencyObject source) where T : class
        {
            var parent = VisualTreeHelper.GetParent(source);
            while (parent != null)
            {
                if (parent is T)
                {
                    return parent as T;
                }
                //if (parent is PopupLayout)
                //{
                //    var popup = ((PopupLayout)parent).Parent as Popup;
                //    var popupParent = PopupHelper.GetPopupParent(popup);
                //    return popupParent != null ? GetParentOfType<T>(popupParent) : null;
                //}
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        public static bool DoesResourceDictionaryContainKey(ResourceDictionary resourceDictionary, object key)
        {
            return resourceDictionary.Contains(key);
        }

        public static void ApplyStyleIfFoundInResources(string style, FrameworkElement element)
        {
            if (DoesResourceDictionaryContainKey(Application.Current.Resources, style))
            {
                element.Style = (Style)Application.Current.Resources[style];
            }
        }

        public static void ApplyDataTemplateIfFoundInResources(string dataTemplateKey, ContentPresenter contentPresenter)
        {
            if (DoesResourceDictionaryContainKey(Application.Current.Resources, dataTemplateKey))
            {
                contentPresenter.ContentTemplate = (DataTemplate)Application.Current.Resources[dataTemplateKey];
            }
        }

        public static object GetResourceIfFound(string resourceKey)
        {
            if (DoesResourceDictionaryContainKey(Application.Current.Resources, resourceKey))
                return Application.Current.Resources[resourceKey];
            else
                return null;
        }

        public static Point TransformPoint(GeneralTransform generalTransform, Point point)
        {
            return generalTransform.Transform(point);
        }

        public static bool CanFirstTypeBeCastedToSecondType(Type firstType, Type secondType)
        {
            return firstType.IsAssignableFrom(secondType);
        }

        public static void DispatcherRunAsync(DependencyObject dependencyObject, Action action)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException("dependencyObject");

            if (action == null)
                throw new ArgumentNullException("action");

            dependencyObject.Dispatcher.BeginInvoke(action);
        }



        public static void DispatcherRunAsync(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            Deployment.Current.Dispatcher.BeginInvoke(action);
        }



        public static void SetFocus(Control control)
        {
            control.Focus();
        }

        public static void RunAfterDelay(Action myMethod, int delayInMilliseconds)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) => Thread.Sleep(delayInMilliseconds);
            worker.RunWorkerCompleted += (s, e) => myMethod.Invoke();
            worker.RunWorkerAsync();
        }


        #region CONVERTION BETWEEN POINTER AND MOUSE EVENTS //--------------------------------------

        public static Dictionary<Tuple<UIElement, PointerEventHandler>, MouseButtonHandlerClass> PointerHandlers = new Dictionary<Tuple<UIElement, PointerEventHandler>, MouseButtonHandlerClass>(); // "public" because must be in AAC

        public static void AttachPointerPressedEventHandler(UIElement uielement, MetroHelpers.PointerEventHandler handler, bool handledEventsToo)
        {
            MouseButtonHandlerClass handlerClass = CreateHandlerForAttachingEvent(uielement, handler);

            uielement.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(handlerClass.UIElement_MouseButton), handledEventsToo);
        }

        public static void AttachPointerReleasedEventHandler(UIElement uielement, MetroHelpers.PointerEventHandler handler, bool handledEventsToo)
        {
            MouseButtonHandlerClass handlerClass = CreateHandlerForAttachingEvent(uielement, handler);

            uielement.AddHandler(FrameworkElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(handlerClass.UIElement_MouseButton), handledEventsToo);
        }

        static MouseButtonHandlerClass CreateHandlerForAttachingEvent(UIElement uielement, MetroHelpers.PointerEventHandler handler)
        {
            if (!PointerHandlers.ContainsKey(new Tuple<UIElement, PointerEventHandler>(uielement, handler)))
            {
                MouseButtonHandlerClass handlerClass = new MouseButtonHandlerClass(handler);
                PointerHandlers.Add(new Tuple<UIElement, PointerEventHandler>(uielement, handler), handlerClass);
                return handlerClass;
            }
            else
                throw new InvalidOperationException("The event cannot be registered multiple times.");
        }

        public static void RemovePointerPressedEventHandler(UIElement uielement, MetroHelpers.PointerEventHandler handler)
        {
            MouseButtonHandlerClass handlerClass = GetMouseButtonHandlerAndRemoveIt(uielement, handler);
            if (handlerClass != null)
            {
                uielement.RemoveHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(handlerClass.UIElement_MouseButton));
            }
        }

        public static void RemovePointerReleasedEventHandler(UIElement uielement, MetroHelpers.PointerEventHandler handler)
        {
            MouseButtonHandlerClass handlerClass = GetMouseButtonHandlerAndRemoveIt(uielement, handler);
            if (handlerClass != null)
            {
                uielement.RemoveHandler(FrameworkElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(handlerClass.UIElement_MouseButton));
            }
        }

        static MouseButtonHandlerClass GetMouseButtonHandlerAndRemoveIt(UIElement uielement, MetroHelpers.PointerEventHandler handler)
        {
            if (PointerHandlers.ContainsKey(new Tuple<UIElement, PointerEventHandler>(uielement, handler)))
            {
                MouseButtonHandlerClass handlerClass = PointerHandlers[new Tuple<UIElement, PointerEventHandler>(uielement, handler)];
                PointerHandlers.Remove(new Tuple<UIElement, PointerEventHandler>(uielement, handler));
                return handlerClass;
            }
            else
                return null;
        }

        public static bool IsPointerPressedEventAttached(UIElement uielement, MetroHelpers.PointerEventHandler handler)
        {
            return PointerHandlers.ContainsKey(new Tuple<UIElement, PointerEventHandler>(uielement, handler));
        }

        public delegate void PointerEventHandler(object sender, PointerRoutedEventArgs e);


        public static PointerRoutedEventArgs CreateNewPointerRoutedEventArgs(MouseButtonEventArgs mouseArgs)
        {
            return new PointerRoutedEventArgs(mouseArgs);
        }

        public class MouseButtonHandlerClass
        {
            PointerEventHandler _handler;

            public MouseButtonHandlerClass(PointerEventHandler handler)
            {
                _handler = handler;
            }

            public void UIElement_MouseButton(object sender, MouseButtonEventArgs mouseArgs) // Works with "MouseLeftButtonDown", "MouseLeftButtonUp", and all other similar mouse events...
            {
                // For silverlight, convert the "MouseButtonEventArgs" into the "PointerRoutedEventArgs" and then back to the "MouseButtonEventArgs":
                PointerRoutedEventArgs pointerArgs = MetroHelpers.CreateNewPointerRoutedEventArgs(mouseArgs);
                _handler(sender, pointerArgs);
                mouseArgs.Handled = pointerArgs.Handled;
            }
        }

        #endregion //-----------------------------------------------------------
    }
}
