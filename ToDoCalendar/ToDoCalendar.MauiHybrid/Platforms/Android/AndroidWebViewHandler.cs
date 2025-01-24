using Microsoft.AspNetCore.Components.WebView.Maui;

namespace ToDoCalendar.MauiHybrid;

public class AndroidWebViewHandler : BlazorWebViewHandler
{
    protected override void ConnectHandler(global::Android.Webkit.WebView webView)
    {
        webView.Settings.SetSupportMultipleWindows(false);

        base.ConnectHandler(webView);
    }
}
