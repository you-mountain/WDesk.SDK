using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WDesk.Core
{
    // ═══════════════════════════════════════════
    //  ENUMS
    // ═══════════════════════════════════════════

    public enum WidgetCategory
    {
        Time, Weather, Productivity, Islamic, System, Custom,
        Music, Finance, Health, Education, Games, Social,
        News, Utilities, Entertainment
    }

    public enum NotificationType { Info, Success, Warning, Error }

    // ═══════════════════════════════════════════
    //  MODELS
    // ═══════════════════════════════════════════

    public class WidgetMetadata
    {
        public string Id { get; set; } = "";
        public string NameKey { get; set; } = "";
        public string DescriptionKey { get; set; } = "";
        public WidgetCategory Category { get; set; } = WidgetCategory.Custom;
        public string Icon { get; set; } = "\uE9D9";
        public string Author { get; set; } = "WDesk";
        public string Version { get; set; } = "1.0.0";
        public double DefaultWidth { get; set; } = 260;
        public double DefaultHeight { get; set; } = 160;
        public bool IsResizable { get; set; } = true;
        public bool HasSettings { get; set; } = false;
    }

    public class PlacedWidget
    {
        public string InstanceId { get; set; } = Guid.NewGuid().ToString("N");
        public string WidgetId { get; set; } = "";
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; } = 260;
        public double Height { get; set; } = 160;
        public double Opacity { get; set; } = 1.0;
        public double Scale { get; set; } = 1.0;
        public bool IsLocked { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        public bool AlwaysOnTop { get; set; } = false;
        public int ZIndex { get; set; } = 0;
        public string StyleId { get; set; } = "";
        public bool BackdropBlur { get; set; } = false;
        public Dictionary<string, string> Settings { get; set; } = new();
    }

    public class WStyle
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public string PreviewEmoji { get; set; } = "";
    }

    // ═══════════════════════════════════════════
    //  INTERFACES
    // ═══════════════════════════════════════════

    public interface IWidget
    {
        WidgetMetadata Metadata { get; }
        FrameworkElement CreateView(PlacedWidget instance);
        void OnActivated(PlacedWidget instance) { }
        void OnDeactivated(PlacedWidget instance) { }
        void ApplySettings(PlacedWidget instance, Dictionary<string, string> settings) { }
    }

    public interface IWidgetWithSettings : IWidget
    {
        FrameworkElement CreateSettingsView(
            PlacedWidget instance,
            Action<Dictionary<string, string>> onSave);
    }

    public interface IWidgetWithStyles : IWidgetWithSettings
    {
        IEnumerable<WStyle> GetStyles();
        string GetCurrentStyle(PlacedWidget instance);
    }

    public interface IStyleBuilder
    {
        FrameworkElement Build(PlacedWidget instance);
    }

    // ═══════════════════════════════════════════
    //  WIDGET BASE
    // ═══════════════════════════════════════════

    public abstract class WidgetBase : IWidgetWithStyles
    {
        public abstract WidgetMetadata Metadata { get; }

        public virtual IEnumerable<WStyle> GetStyles()
        {
            return new List<WStyle>
            {
                new()
                {
                    Id = "style1",
                    Name = "Default",
                    Icon = "\uE8F1",
                    PreviewEmoji = "🎨"
                }
            };
        }

        public virtual IStyleBuilder? GetStyleBuilder(string styleId) => null;

        public string GetCurrentStyle(PlacedWidget instance)
        {
            if (instance.Settings.TryGetValue("style", out var s) &&
                !string.IsNullOrEmpty(s))
                return s;

            return "style1";
        }

        public virtual FrameworkElement CreateView(PlacedWidget instance)
        {
            var style = GetCurrentStyle(instance);
            var builder = GetStyleBuilder(style);

            if (builder == null)
            {
                var first = System.Linq.Enumerable.FirstOrDefault(GetStyles());
                if (first != null)
                    builder = GetStyleBuilder(first.Id);
            }

            return builder?.Build(instance) ?? new Grid();
        }

        public virtual FrameworkElement CreateSettingsView(
            PlacedWidget instance,
            Action<Dictionary<string, string>> onSave)
        {
            return new StackPanel
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = "This widget has no settings.",
                        FontSize = 12,
                        Margin = new Thickness(16)
                    }
                }
            };
        }

        public virtual void ApplySettings(
            PlacedWidget instance,
            Dictionary<string, string> settings)
        { }

        public virtual void OnActivated(PlacedWidget instance) { }
        public virtual void OnDeactivated(PlacedWidget instance) { }
    }
}