using System.Collections.Generic;

namespace System.Web.UI
{
    public enum HtmlTextWriterStyle
    {
        BackgroundColor = 0,
        BackgroundImage = 1,
        BorderCollapse = 2,
        BorderColor = 3,
        BorderStyle = 4,
        BorderWidth = 5,
        Color = 6,
        FontFamily = 7,
        FontSize = 8,
        FontStyle = 9,
        FontWeight = 10,
        Height = 11,
        TextDecoration = 12,
        Width = 13,
        ListStyleImage = 14,
        ListStyleType = 15,
        Cursor = 16,
        Direction = 17,
        Display = 18,
        Filter = 19,
        FontVariant = 20,
        Left = 21,
        Margin = 22,
        MarginBottom = 23,
        MarginLeft = 24,
        MarginRight = 25,
        MarginTop = 26,
        Overflow = 27,
        OverflowX = 28,
        OverflowY = 29,
        Padding = 30,
        PaddingBottom = 31,
        PaddingLeft = 32,
        PaddingRight = 33,
        PaddingTop = 34,
        Position = 35,
        TextAlign = 36,
        VerticalAlign = 37,
        TextOverflow = 38,
        Top = 39,
        Visibility = 40,
        WhiteSpace = 41,
        ZIndex = 42
    }

    public static class HtmlTextWriterStyleExtensions
    {
        static Dictionary<HtmlTextWriterStyle, string> s_attributes = new Dictionary<HtmlTextWriterStyle, string>
        {
            { HtmlTextWriterStyle.BackgroundColor, "background-color" },
            { HtmlTextWriterStyle.BackgroundImage, "background-image" },
            { HtmlTextWriterStyle.BorderCollapse, "border-collapse" },
            { HtmlTextWriterStyle.BorderColor, "border-color" },
            { HtmlTextWriterStyle.BorderStyle, "border-style" },
            { HtmlTextWriterStyle.BorderWidth, "border-width" },
            { HtmlTextWriterStyle.Color, "color" },
            { HtmlTextWriterStyle.Cursor, "cursor" },
            { HtmlTextWriterStyle.Direction, "direction" },
            { HtmlTextWriterStyle.Display, "display" },
            { HtmlTextWriterStyle.Filter, "filter" },
            { HtmlTextWriterStyle.FontFamily, "font-family" },
            { HtmlTextWriterStyle.FontSize, "font-size" },
            { HtmlTextWriterStyle.FontStyle, "font-style" },
            { HtmlTextWriterStyle.FontVariant, "font-variant" },
            { HtmlTextWriterStyle.FontWeight, "font-weight" },
            { HtmlTextWriterStyle.Height, "height" },
            { HtmlTextWriterStyle.Left, "left" },
            { HtmlTextWriterStyle.ListStyleImage, "list-style-image" },
            { HtmlTextWriterStyle.ListStyleType, "list-style-type" },
            { HtmlTextWriterStyle.Margin, "margin" },
            { HtmlTextWriterStyle.MarginBottom, "margin-bottom" },
            { HtmlTextWriterStyle.MarginLeft, "margin-left" },
            { HtmlTextWriterStyle.MarginRight, "margin-right" },
            { HtmlTextWriterStyle.MarginTop, "margin-top" },
            { HtmlTextWriterStyle.Overflow, "overflow" },
            { HtmlTextWriterStyle.OverflowX, "overflow-x" },
            { HtmlTextWriterStyle.OverflowY, "overflow-y" },
            { HtmlTextWriterStyle.Padding, "padding" },
            { HtmlTextWriterStyle.PaddingBottom, "padding-bottom" },
            { HtmlTextWriterStyle.PaddingLeft, "padding-left" },
            { HtmlTextWriterStyle.PaddingRight, "padding-right" },
            { HtmlTextWriterStyle.PaddingTop, "padding-top" },
            { HtmlTextWriterStyle.Position, "position" },
            { HtmlTextWriterStyle.TextAlign, "text-align" },
            { HtmlTextWriterStyle.TextDecoration, "text-decoration" },
            { HtmlTextWriterStyle.TextOverflow, "text-overflow" },
            { HtmlTextWriterStyle.Top, "top" },
            { HtmlTextWriterStyle.VerticalAlign, "vertical-align" },
            { HtmlTextWriterStyle.Visibility, "visibility" },
            { HtmlTextWriterStyle.WhiteSpace, "white-space" },
            { HtmlTextWriterStyle.Width, "width" },
            { HtmlTextWriterStyle.ZIndex, "z-index" },
        };
        public static string ToName(this HtmlTextWriterStyle attributeVal) => s_attributes[attributeVal];
    }
}
