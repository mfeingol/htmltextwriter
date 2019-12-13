// (c) 2019 Max Feingold

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        //
        // Members
        //

        string tabString = DefaultTabString;

        Stack<string> openTags;
        List<KeyValuePair<string, string>> attributes;

        int indent;
        bool lineWasIndented;
        bool pendingCloseTag;

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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                this.InnerWriter.Dispose();
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
        public override string NewLine => this.InnerWriter.NewLine;
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

            if (this.attributes == null)
                this.attributes = new List<KeyValuePair<string, string>>();

            this.attributes.Add(new KeyValuePair<string, string>(name, value));
        }

        //
        // Tags
        //

        public void RenderBeginTag(HtmlTextWriterTag tagKey) => this.RenderBeginTag(tagKey.ToName());

        public void RenderBeginTag(string name)
        {
            this.WriteBeginTag(name);

            if (this.attributes != null)
            {
                foreach (KeyValuePair<string, string> attribute in this.attributes)
                    this.WriteAttribute(attribute.Key, attribute.Value, false); // Already encoded

                this.attributes.Clear();
            }

            if (this.openTags == null)
                this.openTags = new Stack<string>();

            this.openTags.Push(name);

            this.Indent++;
            this.pendingCloseTag = true;
        }

        public void RenderEndTag()
        {
            if (this.openTags == null || !this.openTags.Any())
                throw new InvalidOperationException();

            string tag = this.openTags.Pop();
            this.Indent--;

            if (this.pendingCloseTag)
            {
                this.InnerWriter.WriteLine(SelfClosingTagEnd);
                this.AfterWriteLine();
            }
            else
            {
                if (this.lineWasIndented)
                    this.WriteLine();

                this.WriteLine($"{EndTagLeftChars}{tag}{TagRightChar}");
            }

            this.pendingCloseTag = false;
        }

        void CloseTagIfNecessary(bool newline = true)
        {
            if (this.pendingCloseTag)
            {
                this.InnerWriter.Write(TagRightChar);
                this.pendingCloseTag = false;

                if (newline)
                    this.WriteLine();
            }
        }

        async Task CloseTagIfNecessaryAsync(bool newline = true)
        {
            if (this.pendingCloseTag)
            {
                await this.InnerWriter.WriteAsync(TagRightChar);
                this.pendingCloseTag = false;

                if (newline)
                    await this.WriteLineAsync();
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

        void ResetIndent()
        {
            this.lineWasIndented = false;
        }

        //
        // Coordination
        //

        void BeforeWrite(bool indent = true, bool newLineIfClosingTag = true)
        {
            this.CloseTagIfNecessary(newLineIfClosingTag);
            if (indent)
                this.IndentIfNecessary();
        }

        async Task BeforeWriteAsync(bool indent = true, bool newLineIfClosingTag = true)
        {
            await this.CloseTagIfNecessaryAsync(newLineIfClosingTag);
            if (indent)
                await this.IndentIfNecessaryAsync();
        }

        void AfterWriteLine()
        {
            this.ResetIndent();
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

        public void WriteBeginTag(string name) => this.Write($"{TagLeftChar}{name}");
        public void WriteBreak() => this.Write($"{TagLeftChar}{HtmlTextWriterTag.Br.ToName()}{SelfClosingTagEnd}");
        public void WriteEncodedText(string text) => this.Write(WebUtility.HtmlEncode(text));
        public void WriteEncodedUrl(string url)
        {
            int index = url.IndexOf('?');
            if (index != -1)
                this.Write($"{Uri.EscapeUriString(url.Substring(0, index))}{url.Substring(index)}");
            else
                this.Write(Uri.EscapeUriString(url));
        }

        public void WriteEncodedUrlParameter(string urlText) => this.Write(Uri.EscapeUriString(urlText));
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
            this.BeforeWrite(indent: false, newLineIfClosingTag: false);
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
    }
}
