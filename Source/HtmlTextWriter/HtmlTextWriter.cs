﻿// (c) 2019 Max Feingold

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.UI
{
    public class HtmlTextWriter : TextWriter
    {
        //
        // Constants
        //

        public const string DefaultTabString = "\t";
        public const char DoubleQuoteChar = '"';
        public const string EndTagLeftChars = "</";
        public const char EqualsChar = '=';
        public const string EqualsDoubleQuoteString = "=\"";
        public const string SelfClosingChars = " /";
        public const string SelfClosingTagEnd = " />";
        public const char SemicolonChar = ';';
        public const char SingleQuoteChar = '\'';
        public const char SlashChar = '/';
        public const char SpaceChar = ' ';
        public const char StyleEqualsChar = ':';
        public const char TagLeftChar = '<';
        public const char TagRightChar = '>';
        public const string StyleDeclaringString = "style";

        //
        // Members
        //

        readonly string tabString = DefaultTabString;

        Stack<TagMetadata> openTags;
        List<KeyValuePair<string, string>> attributes;
        List<KeyValuePair<string, string>> styleAttributes;

        int indent;
        bool lineWasIndented;

        //
        // Constructors
        //

        public HtmlTextWriter(TextWriter writer) : this(writer, DefaultTabString)
        {
        }

        public HtmlTextWriter(TextWriter writer, string tabString)
        {
            this.InnerWriter = writer;
            this.tabString = tabString;
        }

        //
        // Properties
        //

        public TextWriter InnerWriter { get; set; }

        public int Indent
        {
            get => this.indent;
            set
            {
                if (value < 0)
                    value = 0;

                this.indent = value;
            }
        }

        public override string NewLine
        {
            get => this.InnerWriter.NewLine;
            set => this.InnerWriter.NewLine = value;
        }

        public override Encoding Encoding => this.InnerWriter.Encoding;

        //
        // Begin, end
        //

        public void BeginRender() { }
        public void EndRender() { }

        //
        // AddAttribute
        //

        public void AddAttribute(HtmlTextWriterAttribute key, string value) => this.AddAttribute(key, value, true);
        public void AddAttribute(HtmlTextWriterAttribute key, string value, bool encode) => AddAttribute(key.ToName(), value, encode);

        public void AddAttribute(string name, string value) => this.AddAttribute(name, value, true);
        public void AddAttribute(string name, string value, bool encode)
        {
            if (encode)
                value = WebUtility.HtmlEncode(value);

            this.attributes ??= [];
            this.attributes.Add(new KeyValuePair<string, string>(name, value));
        }
        public void AddStyleAttribute(HtmlTextWriterStyle key, string value) => this.AddStyleAttribute(key, value, true);
        public void AddStyleAttribute(HtmlTextWriterStyle key, string value, bool encode) => AddStyleAttribute(key.ToName(), value, encode);

        [Obsolete("Typo in method name preserved for binary compatibility. Use the corrected method AddStyleAttribute(string, string).")]
        public void AddStyleAttrbiute(string name, string value) => this.AddStyleAttribute(name, value);
        public void AddStyleAttribute(string name, string value) => this.AddStyleAttribute(name, value, true);
        public void AddStyleAttribute(string name, string value, bool encode)
        {
            if (encode)
                value = WebUtility.HtmlEncode(value);

            this.styleAttributes ??= [];
            this.styleAttributes.Add(new KeyValuePair<string, string>(name, value));
        }

        //
        // Tags
        //

        public void RenderBeginTag(HtmlTextWriterTag tagKey) => this.RenderBeginTagInternal(tagKey.ToMetadata());

        public void RenderBeginTag(string name)
        {
            string lower = name.ToLowerInvariant();
            if (name != lower && lower.IsKnownTag())
                name = lower;

            this.RenderBeginTagInternal(name.ToMetadata());
        }

        void RenderBeginTagInternal(TagMetadata metadata)
        {
            this.WriteBeginTag(metadata.Name);

            if (this.attributes != null)
            {
                foreach (KeyValuePair<string, string> attribute in this.attributes)
                    this.WriteAttribute(attribute.Key, attribute.Value, false); // Already encoded

                this.attributes.Clear();
            }

            if (this.styleAttributes != null && this.styleAttributes.Count != 0)
            {
                this.Write($"{SpaceChar}{StyleDeclaringString}{EqualsDoubleQuoteString}");
                foreach (KeyValuePair<string, string> styleAttribute in this.styleAttributes)
                    this.WriteStyleAttribute(styleAttribute.Key, styleAttribute.Value, false);

                this.Write(DoubleQuoteChar);
                this.styleAttributes.Clear();
            }

            switch (metadata.OpenBehavior)
            {
                case BeginTagBehavior.OpenTag:
                    this.Write(TagRightChar);
                    break;
                case BeginTagBehavior.OpenTagWithLineBreak:
                    this.Write(TagRightChar);
                    this.WriteLine();
                    break;
                case BeginTagBehavior.SelfClose:
                    this.Write(SelfClosingTagEnd);
                    break;
            }

            this.openTags ??= new();
            this.openTags.Push(metadata);

            if (metadata.IndentBehavior == BeginTagIndentBehavior.Indent)
                this.Indent++;
        }

        public void RenderEndTag()
        {
            if (this.openTags == null || this.openTags.Count == 0)
                throw new InvalidOperationException();

            TagMetadata metadata = this.openTags.Pop();

            if (metadata.IndentBehavior == BeginTagIndentBehavior.Indent)
                this.Indent--;

            switch (metadata.OpenBehavior)
            {
                case BeginTagBehavior.OpenTag:
                    this.Write($"{EndTagLeftChars}{metadata.Name}{TagRightChar}");
                    break;
                case BeginTagBehavior.OpenTagWithLineBreak:
                    this.WriteLine();
                    this.Write($"{EndTagLeftChars}{metadata.Name}{TagRightChar}");
                    break;
                case BeginTagBehavior.SelfClose:
                    // No-op
                    break;
            }
        }

        //
        // Indents
        //

        void IndentIfNecessary()
        {
            if (!this.lineWasIndented)
            {
                for (int i = 0; i < this.Indent; i++)
                    this.InnerWriter.Write(this.tabString);

                this.lineWasIndented = true;
            }
        }

        async Task IndentIfNecessaryAsync()
        {
            if (!this.lineWasIndented)
            {
                for (int i = 0; i < this.Indent; i++)
                    await this.InnerWriter.WriteAsync(this.tabString);

                this.lineWasIndented = true;
            }
        }

        //
        // Coordination
        //

        void BeforeWrite(bool indent = true)
        {
            if (indent)
                this.IndentIfNecessary();
        }

        async Task BeforeWriteAsync(bool indent = true)
        {
            if (indent)
                await this.IndentIfNecessaryAsync();
        }

        void AfterWriteLine()
        {
            this.lineWasIndented = false;
        }

        //
        // HTML-specific writes
        //

        public void WriteAttribute(string name, string value) => this.WriteAttribute(name, value, true);
        public void WriteAttribute(string name, string value, bool encode)
        {
            this.Write($"{SpaceChar}{name}");

            if (value != null)
            {
                if (encode)
                    value = WebUtility.HtmlEncode(value);

                this.Write($"{EqualsDoubleQuoteString}{value}{DoubleQuoteChar}");
            }
        }

        public void WriteStyleAttribute(string name, string value) => this.WriteStyleAttribute(name, value, true);
        public void WriteStyleAttribute(string name, string value, bool encode)
        {
            if (name != null && value != null)
            {
                this.Write($"{name}{StyleEqualsChar}");
                if (encode)
                    value = WebUtility.HtmlEncode(value);

                this.Write($"{value}{SemicolonChar}");
            }
        }

        public void WriteBeginTag(string name) => this.Write($"{TagLeftChar}{name}");
        public void WriteBreak() => this.Write($"{TagLeftChar}{HtmlTextWriterTag.Br.ToMetadata().Name}{SelfClosingTagEnd}");
        public void WriteEncodedText(string text) => this.Write(WebUtility.HtmlEncode(text));
        public void WriteEncodedUrl(string url)
        {
            int index = url.IndexOf('?');
            if (index != -1)
#if NET8_0_OR_GREATER
                this.Write($"{Uri.EscapeDataString(url[..index])}{url.AsSpan(index..)}");
#else
                this.Write($"{Uri.EscapeDataString(url.Substring(0, index))}{url.Substring(index)}");
#endif
            else
                this.Write(Uri.EscapeDataString(url));
        }

        public void WriteEncodedUrlParameter(string urlText) => this.Write(Uri.EscapeDataString(urlText));
        public void WriteEndTag(string tagName) => this.Write($"{TagLeftChar}{SlashChar}{tagName}{TagRightChar}");
        public void WriteFullBeginTag(string tagName) => this.Write($"{TagLeftChar}{tagName}{TagRightChar}");
        public void WriteLineNoTabs(string line) => this.WriteLine(line);

        //
        // Close/Flush
        //

#if NETSTANDARD1_4
        public void Close() { }
#else
        public override void Close() => this.InnerWriter.Close();
#endif
        public override void Flush() => this.InnerWriter.Flush();
        public override Task FlushAsync() => this.InnerWriter.FlushAsync();

        //
        // Write
        //

        public override void Write(ulong value)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(value);
        }

        public override void Write(uint value)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(value);
        }

        public override void Write(string format, params object[] arg)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(format, arg);
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(format, arg0, arg1, arg2);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(format, arg0, arg1);
        }

        public override void Write(string format, object arg0)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(format, arg0);
        }

        public override void Write(string value)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(value);
        }

        public override void Write(object value)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(value);
        }

        public override void Write(long value)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(value);
        }

        public override void Write(int value)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(value);
        }

        public override void Write(double value)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(value);
        }

        public override void Write(decimal value)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(buffer, index, count);
        }

        public override void Write(char[] buffer)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(buffer);
        }

        public override void Write(char value)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(value);
        }

        public override void Write(bool value)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(value);
        }

        public override void Write(float value)
        {
            this.BeforeWrite();
            this.InnerWriter.Write(value);
        }

        public override async Task WriteAsync(string value)
        {
            await this.BeforeWriteAsync();
            await this.InnerWriter.WriteAsync(value);
        }

        public override async Task WriteAsync(char value)
        {
            await this.BeforeWriteAsync();
            await this.InnerWriter.WriteAsync(value);
        }

        public override async Task WriteAsync(char[] buffer, int index, int count)
        {
            await this.BeforeWriteAsync();
            await this.InnerWriter.WriteAsync(buffer, index, count);
        }

        //
        // WriteLine
        //

        public override void WriteLine(string format, object arg0)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(format, arg0);
            this.AfterWriteLine();
        }

        public override void WriteLine(ulong value)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(value);
            this.AfterWriteLine();
        }

        public override void WriteLine(uint value)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(value);
            this.AfterWriteLine();
        }

        public override void WriteLine(string format, params object[] arg)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(format, arg);
            this.AfterWriteLine();
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(format, arg0, arg1, arg2);
            this.AfterWriteLine();
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(format, arg0, arg1);
            this.AfterWriteLine();
        }

        public override void WriteLine(string value)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(value);
            this.AfterWriteLine();
        }

        public override void WriteLine(float value)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(value);
            this.AfterWriteLine();
        }

        public override void WriteLine()
        {
            this.BeforeWrite(indent: false);
            this.InnerWriter.WriteLine();
            this.AfterWriteLine();
        }

        public override void WriteLine(long value)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(value);
            this.AfterWriteLine();
        }

        public override void WriteLine(int value)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(value);
            this.AfterWriteLine();
        }

        public override void WriteLine(double value)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(value);
            this.AfterWriteLine();
        }

        public override void WriteLine(decimal value)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(value);
            this.AfterWriteLine();
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(buffer, index, count);
            this.AfterWriteLine();
        }

        public override void WriteLine(char[] buffer)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(buffer);
            this.AfterWriteLine();
        }

        public override void WriteLine(char value)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(value);
            this.AfterWriteLine();
        }

        public override void WriteLine(bool value)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(value);
            this.AfterWriteLine();
        }

        public override void WriteLine(object value)
        {
            this.BeforeWrite();
            this.InnerWriter.WriteLine(value);
            this.AfterWriteLine();
        }

        public override async Task WriteLineAsync()
        {
            await this.BeforeWriteAsync();
            await this.InnerWriter.WriteLineAsync();
            this.AfterWriteLine();
        }

        public override async Task WriteLineAsync(char value)
        {
            await this.BeforeWriteAsync();
            await this.InnerWriter.WriteLineAsync(value);
            this.AfterWriteLine();
        }

        public override async Task WriteLineAsync(char[] buffer, int index, int count)
        {
            await this.BeforeWriteAsync();
            await this.InnerWriter.WriteLineAsync(buffer, index, count);
            this.AfterWriteLine();
        }

        public override async Task WriteLineAsync(string value)
        {
            await this.BeforeWriteAsync();
            await this.InnerWriter.WriteLineAsync(value);
            this.AfterWriteLine();
        }

#if NET8_0_OR_GREATER
        // Performance optimized version of Write method overload.
        void Write(HtmlTextWriteInterpolatedStringHandler handler)
        {
            this.BeforeWrite();
            handler.Write(this.InnerWriter);
        }

        const int ValueCharBufferLength = 256;

        [InterpolatedStringHandler]
        ref struct HtmlTextWriteInterpolatedStringHandler
        {
            // Storage for data
            ValueCharBuffer spanBuffer; // On stack
            StringBuilder builder; // Fallback on heap
            int spanBufferIndex;

            public HtmlTextWriteInterpolatedStringHandler(int literalLength, int formattedCount)
            {
                spanBuffer = new();
            }

            public void AppendLiteral(string s)
            {
                this.AppendFormatted(s);
            }

            public void AppendFormatted(ReadOnlySpan<char> s)
            {
                this.EnsureBuffer(s.Length);

                if (builder != null)
                {
                    builder.Append(s);
                }
                else
                {
                    s.CopyTo(spanBuffer[spanBufferIndex..]);
                    spanBufferIndex += s.Length;
                }
            }

            public void AppendFormatted(char c)
            {
                this.EnsureBuffer(1);

                if (builder != null)
                {
                    builder.Append(c);
                }
                else
                {
                    spanBuffer[spanBufferIndex] = c;
                    spanBufferIndex++;
                }
            }

            void EnsureBuffer(int length)
            {
                if (spanBufferIndex == -1)
                    return;

                if (spanBufferIndex + length > ValueCharBufferLength)
                {
                    builder = new(ValueCharBufferLength * 2);
                    if (spanBufferIndex > 0)
                        builder.Append(spanBuffer[..spanBufferIndex]);

                    spanBufferIndex = -1;
                }
            }

            internal readonly void Write(TextWriter innerWriter)
            {
                if (builder != null)
                {
                    foreach (var chunk in builder.GetChunks())
                        innerWriter.Write(chunk);
                }
                else
                {
                    innerWriter.Write(spanBuffer[..spanBufferIndex]);
                }
            }
        }

        // Struct to hold char buffer on the stack leading to zero allocations from HtmlTextWriter.
        [InlineArray(ValueCharBufferLength)]
        struct ValueCharBuffer
        {
            char _element0;
        }
#endif
    }
}
