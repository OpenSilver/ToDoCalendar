using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
    }
}
