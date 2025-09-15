using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace GabsHybridApp.Maui.Services;

public static class StatusBarThemeExtensions
{
    /// <summary>
    /// Sets the system status bar color app-wide.
    /// <paramref name="hex"/> = #RRGGBB or #AARRGGBB.
    /// <paramref name="lightContent"/> = true for white icons/text; false for dark icons.
    /// </summary>
    public static MauiAppBuilder UseStatusBarTheme(this MauiAppBuilder builder, string hex, bool lightContent)
    {
        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) =>
            {
                var window = activity?.Window;
                if (window is null) return;

                var color = Android.Graphics.Color.ParseColor(NormalizeHex(hex));
                window.SetStatusBarColor(color);
                // Optional: also match the nav bar
                // window.SetNavigationBarColor(color);

                var ctl = new AndroidX.Core.View.WindowInsetsControllerCompat(window, window.DecorView);
                // false => light icons; true => dark icons
                ctl.AppearanceLightStatusBars = !lightContent;
                ctl.AppearanceLightNavigationBars = !lightContent;
            }));
#elif IOS
            events.AddiOS(ios => ios.FinishedLaunching((app, _) =>
            {
                var color = FromHex(hex);

                var appearance = new UIKit.UINavigationBarAppearance();
                appearance.ConfigureWithOpaqueBackground();
                appearance.BackgroundColor = color;

                UIKit.UINavigationBar.Appearance.StandardAppearance   = appearance;
                UIKit.UINavigationBar.Appearance.ScrollEdgeAppearance = appearance;

                UIKit.UIApplication.SharedApplication.StatusBarStyle =
                    lightContent ? UIKit.UIStatusBarStyle.LightContent
                                 : UIKit.UIStatusBarStyle.DarkContent;

                return true;
            }));
#endif
        });

        return builder;
    }

#if ANDROID
    private static string NormalizeHex(string h) =>
        string.IsNullOrWhiteSpace(h) ? "#000000" : (h.StartsWith('#') ? h : $"#{h}");
#endif

#if IOS
    private static UIKit.UIColor FromHex(string hex)
    {
        if (!hex.StartsWith("#")) hex = "#" + hex;
        byte a = 255, r, g, b;
        if (hex.Length == 7)
        {
            r = Convert.ToByte(hex.Substring(1, 2), 16);
            g = Convert.ToByte(hex.Substring(3, 2), 16);
            b = Convert.ToByte(hex.Substring(5, 2), 16);
        }
        else if (hex.Length == 9)
        {
            a = Convert.ToByte(hex.Substring(1, 2), 16);
            r = Convert.ToByte(hex.Substring(3, 2), 16);
            g = Convert.ToByte(hex.Substring(5, 2), 16);
            b = Convert.ToByte(hex.Substring(7, 2), 16);
        }
        return UIKit.UIColor.FromRGBA(r, g, b, a);
    }
#endif
}
