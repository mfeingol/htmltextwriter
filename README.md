# HtmlTextWriter

## What is this for?

HtmlTextWriter is a simple, drop-in replacement for the .NET Framework's [System.Web.UI.HtmlWriter](https://docs.microsoft.com/en-us/dotnet/api/system.web.ui.htmltextwriter?view=netframework-4.8) class, which is not yet included in .NET Standard.

## Target uses

This library is intended for apps ported from .NET Framework to Xamarin, UWP, or .NET Core in general, and which use System.Web.UI.HtmlWriter to generate simple HTML content. If you're writing new code in .NET Core, presumably there's an ASP.NET Core thing you can use instead.

## Gaps

This library is a reimplementation, and may not be character-for-character compatible with the original. E.g. linebreak handling may be subtly different. That's probably okay.

The original HtmlTextWriter has a number of features that this one doesn't support yet:

- Styles
- Non-closing tags, such as `<hr>` or `<meta>`
- Most of the protected members used by derived classes.

If your scenarios require these things, or if you find a bug, feel free to submit a PR.
