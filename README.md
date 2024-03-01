# HtmlTextWriter

## What is this for?

HtmlTextWriter is a simple, drop-in replacement for the .NET Framework's [System.Web.UI.HtmlWriter](https://docs.microsoft.com/en-us/dotnet/api/system.web.ui.htmltextwriter?view=netframework-4.8) class, which is not yet included in .NET Standard.

## Target uses

This library implements the .NET Framework System.Web.UI.HtmlWriter API to generate simple HTML content. HtmlWriter was not carried forward to more modern versions of .NET, so this library is intended to help with porting older apps. If you're writing new .NET code, then presumably there's an ASP.NET thing you can use instead.

## Gaps

This library is a clean room reimplementation, and may not be character-for-character compatible with the original.

The original HtmlTextWriter has a number of features that this one doesn't support yet, including many of the protected members that could have been used by derived classes. If your scenarios require these things, or if you find a bug, feel free to submit a PR.
