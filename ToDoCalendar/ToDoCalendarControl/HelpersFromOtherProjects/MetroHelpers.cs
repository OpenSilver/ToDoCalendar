/* <License>The source code below is the property of Userware and is strictly confidential. It is licensed to OP.SERV under agreement 'USE-200-CLM-OPS'</License> */

#if SILVERLIGHT
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WinRTForSilverlight;
#elif WINRT
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#endif

namespace MetroStyleApps
{
    public class MetroHelpers
    {
        public static UIElement GetRootVisual()
        {
#if WINRT
        return Window.Current.Content;
#else
#if SILVERLIGHT
            return Application.Current.RootVisual;
#endif
#endif
            return null;
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
#if WINRT
    return resourceDictionary.ContainsKey(key);
#else
            return resourceDictionary.Contains(key);
#endif
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
#if WINRT
            return generalTransform.TransformPoint(point);
#else
            return generalTransform.Transform(point);
#endif
        }

        public static bool CanFirstTypeBeCastedToSecondType(Type firstType, Type secondType)
        {
#if WINRT
            return secondType.GetTypeInfo().IsAssignableFrom(firstType.GetTypeInfo());
#else
            return firstType.IsAssignableFrom(secondType);
#endif
        }

        public static void DispatcherRunAsync(DependencyObject dependencyObject, Action action)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException("dependencyObject");

            if (action == null)
                throw new ArgumentNullException("action");

#if WINRT
        dependencyObject.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
        () =>
        {
            action();
        });
#else
            dependencyObject.Dispatcher.BeginInvoke(action);
#endif
        }



        public static void DispatcherRunAsync(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

#if WINRT
        Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
        () =>
        {
            action();
        });
#else
            Deployment.Current.Dispatcher.BeginInvoke(action);
#endif
        }



        public static void SetFocus(Control control)
        {
#if WINRT
            control.Focus(Windows.UI.Xaml.FocusState.Programmatic);
#else
            control.Focus();
#endif
        }

#if WINRT
        public static async
#else
        public static 
#endif
                void RunAfterDelay(Action myMethod, int delayInMilliseconds)
        {
#if WINRT
            await Task.Delay(delayInMilliseconds); //todo: test this line.
            myMethod();
#else
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) => Thread.Sleep(delayInMilliseconds);
            worker.RunWorkerCompleted += (s, e) => myMethod.Invoke();
            worker.RunWorkerAsync();
#endif
        }


        #region CONVERTION BETWEEN POINTER AND MOUSE EVENTS //--------------------------------------

        public static Dictionary<Tuple<UIElement, PointerEventHandler>, MouseButtonHandlerClass> PointerHandlers = new Dictionary<Tuple<UIElement, PointerEventHandler>, MouseButtonHandlerClass>(); // "public" because must be in AAC

        public static void AttachPointerPressedEventHandler(UIElement uielement, MetroHelpers.PointerEventHandler handler, bool handledEventsToo)
        {
            MouseButtonHandlerClass handlerClass = CreateHandlerForAttachingEvent(uielement, handler);

#if WINRT
        uielement.AddHandler(FrameworkElement.PointerPressedEvent, new PointerEventHandler(handler), handledEventsToo);
#else
            uielement.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(handlerClass.UIElement_MouseButton), handledEventsToo);
#endif
        }

        public static void AttachPointerReleasedEventHandler(UIElement uielement, MetroHelpers.PointerEventHandler handler, bool handledEventsToo)
        {
            MouseButtonHandlerClass handlerClass = CreateHandlerForAttachingEvent(uielement, handler);

#if WINRT
        uielement.AddHandler(FrameworkElement.PointerReleasedEvent, new PointerEventHandler(handler), handledEventsToo);
#else
            uielement.AddHandler(FrameworkElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(handlerClass.UIElement_MouseButton), handledEventsToo);
#endif
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
#if WINRT
            uielement.RemoveHandler(FrameworkElement.PointerPressedEvent, new PointerEventHandler(handler));
#else
                uielement.RemoveHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(handlerClass.UIElement_MouseButton));
#endif
            }
        }

        public static void RemovePointerReleasedEventHandler(UIElement uielement, MetroHelpers.PointerEventHandler handler)
        {
            MouseButtonHandlerClass handlerClass = GetMouseButtonHandlerAndRemoveIt(uielement, handler);
            if (handlerClass != null)
            {
#if WINRT
            uielement.RemoveHandler(FrameworkElement.PointerReleasedEvent, new PointerEventHandler(handler));
#else
                uielement.RemoveHandler(FrameworkElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(handlerClass.UIElement_MouseButton));
#endif
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

        //todo: when supported by the compiler, put the following delegate in a file on its own and place that file to the "WinRTForSilverlight" project
        public delegate void PointerEventHandler(object sender, PointerRoutedEventArgs e);


#if WINRT
    // No helper needed
#else
#if SILVERLIGHT
        public static PointerRoutedEventArgs CreateNewPointerRoutedEventArgs(MouseButtonEventArgs mouseArgs)
        {
            return new PointerRoutedEventArgs(mouseArgs);
        }
#endif
#endif

        public class MouseButtonHandlerClass
        {
            PointerEventHandler _handler;

            public MouseButtonHandlerClass(PointerEventHandler handler)
            {
                _handler = handler;
            }

#if WINRT
    // No handler needed
#else
#if SILVERLIGHT
            public void UIElement_MouseButton(object sender, MouseButtonEventArgs mouseArgs) // Works with "MouseLeftButtonDown", "MouseLeftButtonUp", and all other similar mouse events...
            {
                // For silverlight, convert the "MouseButtonEventArgs" into the "PointerRoutedEventArgs" and then back to the "MouseButtonEventArgs":
                PointerRoutedEventArgs pointerArgs = MetroHelpers.CreateNewPointerRoutedEventArgs(mouseArgs);
                _handler(sender, pointerArgs);
                mouseArgs.Handled = pointerArgs.Handled;
            }
#endif
#endif
        }

        #endregion //-----------------------------------------------------------




    }
}
