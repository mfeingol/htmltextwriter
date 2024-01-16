// (c) 2019 Max Feingold

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.UI.Tests
{
    [TestClass]
    public class HtmlTextWriterTests
    {
        [TestMethod]
        public void EnsureEnumerations()
        {
            WriteDictionaryEntries<HtmlTextWriterTag>();
            WriteDictionaryEntries<HtmlTextWriterAttribute>();

            using MemoryStream mem = new MemoryStream();
            using StreamWriter sw = new StreamWriter(mem);
            using HtmlTextWriter writer = new HtmlTextWriter(sw);

            foreach (FieldInfo tagInfo in typeof(HtmlTextWriterTag).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                HtmlTextWriterTag tag = (HtmlTextWriterTag)tagInfo.GetRawConstantValue();
                if (tag == HtmlTextWriterTag.Unknown)
                    continue;

                writer.RenderBeginTag(tag);

                foreach (FieldInfo attrInfo in typeof(HtmlTextWriterAttribute).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    HtmlTextWriterAttribute attr = (HtmlTextWriterAttribute)attrInfo.GetRawConstantValue();
                    writer.AddAttribute(attr, "1");
                }

                writer.RenderEndTag();
            }

            // If we make it here without any exceptions, we're good

            static void WriteDictionaryEntries<T>() where T : Enum
            {
                foreach (FieldInfo tagInfo in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    T tag = (T)tagInfo.GetRawConstantValue();
                    Debug.WriteLine($"{{ {typeof(T).Name}.{tag}, \"{tag.ToString().ToLower()}\" }},");
                }
            }
        }

        [TestMethod]
        public void TestMicrosoftDocs()
        {
            using MemoryStream mem = new MemoryStream();
            using StreamWriter sw = new StreamWriter(mem);
            using HtmlTextWriter writer = new HtmlTextWriter(sw);

            // Adapted from https://docs.microsoft.com/en-us/dotnet/api/system.web.ui.htmltextwriter?view=netframework-4.8
            writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "alert('Hello');");
            writer.AddAttribute("CustomAttribute", "CustomAttributeValue");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);

            writer.WriteLine();
            writer.Indent++;
            writer.Write("Hello");
            writer.WriteLine();

            writer.AddAttribute(HtmlTextWriterAttribute.Alt, "Encoding, \"Required\"", true);
            writer.AddAttribute("myattribute", "No &quot;encoding &quot; required", false);
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
            writer.WriteLine();

            writer.RenderBeginTag("MyTag");
            writer.Write("Contents of MyTag");
            writer.RenderEndTag();
            writer.WriteLine();

            writer.WriteBeginTag("img");
            writer.WriteAttribute("alt", "A custom image.");
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.WriteEndTag("img");
            writer.WriteLine();

            writer.Indent--;
            writer.RenderEndTag();

            writer.Flush();
            mem.Position = 0;

            string html = Encoding.UTF8.GetString(mem.ToArray());

            const string test = "<span onclick=\"alert(&#39;Hello&#39;);\" CustomAttribute=\"CustomAttributeValue\">\r\n\t\tHello\r\n\t\t<img alt=\"Encoding, &quot;Required&quot;\" myattribute=\"No &quot;encoding &quot; required\" />\r\n\r\n\t\t<MyTag>\r\n\t\t\tContents of MyTag\r\n\t\t</MyTag>\r\n\r\n\t\t<img alt=\"A custom image.\"></img>\r\n</span>\r\n";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestScenario1()
        {
            using MemoryStream mem = new MemoryStream();
            using StreamWriter sw = new StreamWriter(mem);
            using HtmlTextWriter writer = new HtmlTextWriter(sw);

            // Custom scenario from private code
            writer.RenderBeginTag(HtmlTextWriterTag.Html);
            writer.RenderBeginTag(HtmlTextWriterTag.Center);

            foreach (int i in new[] { 1, 2, 3, 4, 5 })
            {
                writer.RenderBeginTag(HtmlTextWriterTag.P);

                writer.RenderBeginTag(HtmlTextWriterTag.H2);
                writer.Write(i);
                writer.RenderEndTag();

                foreach (string u in new[] { "/a", "/b", "/c" })
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, u);
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, "640");
                    writer.AddAttribute(HtmlTextWriterAttribute.Height, "480");
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
                writer.WriteLine();
            }
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.Flush();
            mem.Position = 0;

            string html = Encoding.UTF8.GetString(mem.ToArray());

            const string test = "<html>\r\n\t<center>\r\n\t\t<p>\r\n\t\t\t<h2>\r\n\t\t\t\t1\r\n\t\t\t</h2>\r\n\t\t\t<img src=\"/a\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/b\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/c\" width=\"640\" height=\"480\" />\r\n\t\t</p>\r\n\r\n\t\t<p>\r\n\t\t\t<h2>\r\n\t\t\t\t2\r\n\t\t\t</h2>\r\n\t\t\t<img src=\"/a\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/b\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/c\" width=\"640\" height=\"480\" />\r\n\t\t</p>\r\n\r\n\t\t<p>\r\n\t\t\t<h2>\r\n\t\t\t\t3\r\n\t\t\t</h2>\r\n\t\t\t<img src=\"/a\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/b\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/c\" width=\"640\" height=\"480\" />\r\n\t\t</p>\r\n\r\n\t\t<p>\r\n\t\t\t<h2>\r\n\t\t\t\t4\r\n\t\t\t</h2>\r\n\t\t\t<img src=\"/a\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/b\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/c\" width=\"640\" height=\"480\" />\r\n\t\t</p>\r\n\r\n\t\t<p>\r\n\t\t\t<h2>\r\n\t\t\t\t5\r\n\t\t\t</h2>\r\n\t\t\t<img src=\"/a\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/b\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/c\" width=\"640\" height=\"480\" />\r\n\t\t</p>\r\n\r\n\t</center>\r\n</html>\r\n";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestScenario2()
        {
            using MemoryStream mem = new MemoryStream();
            using StreamWriter sw = new StreamWriter(mem);
            using HtmlTextWriter writer = new HtmlTextWriter(sw);

            // Custom scenario from private code
            writer.RenderBeginTag(HtmlTextWriterTag.Html);

            // <h2>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            writer.RenderBeginTag(HtmlTextWriterTag.H2);
            writer.Write("TestScenario2");
            writer.RenderEndTag();
            // </h2>

            // <p>
            writer.RenderBeginTag(HtmlTextWriterTag.P);

            // <h3>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            writer.RenderBeginTag(HtmlTextWriterTag.H3);

            writer.Write("Hello {0}", "World");

            writer.RenderEndTag();
            writer.RenderEndTag();

            // <p>
            writer.RenderBeginTag(HtmlTextWriterTag.P);

            // <table>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "1");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "1024");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            // <tr>
            writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "3");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            // <font>
            writer.AddAttribute("face", "Times New Roman");
            writer.RenderBeginTag(HtmlTextWriterTag.Font);
            writer.Write("A nice summary of things");
            writer.RenderEndTag();
            // </font>

            writer.RenderEndTag();
            // </th>

            writer.RenderEndTag();
            // </tr>

            // Begin, end times
            // <tr>
            writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.AddAttribute("face", "Times New Roman");
            writer.RenderBeginTag(HtmlTextWriterTag.Font);
            writer.Write("Began");
            writer.RenderEndTag();

            // </th>
            writer.RenderEndTag();

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.Write("12:00pm");

            // </th>
            writer.RenderEndTag();

            // </tr>
            writer.RenderEndTag();

            // <tr>
            writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.AddAttribute("face", "Times New Roman");
            writer.RenderBeginTag(HtmlTextWriterTag.Font);
            writer.Write("Ended");
            writer.RenderEndTag();

            // </th>
            writer.RenderEndTag();

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);
            writer.Write("1:00pm");

            // </th>
            writer.RenderEndTag();

            // </tr>
            writer.RenderEndTag();

            // <tr>
            writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.AddAttribute("face", "arial");
            writer.RenderBeginTag(HtmlTextWriterTag.Font);
            writer.Write("Elapsed");
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.Write(TimeSpan.FromHours(1).ToString());

            writer.RenderEndTag();

            // </tr>
            writer.RenderEndTag();

            // <tr>
            writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.AddAttribute("face", "Times New Roman");
            writer.RenderBeginTag(HtmlTextWriterTag.Font);
            writer.Write("Result");
            writer.RenderEndTag();

            // </th>
            writer.RenderEndTag();

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.Write("Great Success!");

            // </th>
            writer.RenderEndTag();

            // </tr>
            writer.RenderEndTag();

            // <tr>
            writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.AddAttribute("face", "Times New Roman");
            writer.RenderBeginTag(HtmlTextWriterTag.Font);
            writer.Write("A wonderful description");
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.Write("Some text");
            writer.RenderEndTag();

            // </tr>
            writer.RenderEndTag();

            // Download rows
            for (int i = 0; i < 10; i++)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write("/a/b/c");
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write("26 files");
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write("200GB");
                writer.RenderEndTag();

                writer.RenderEndTag();
            }

            writer.RenderEndTag();
            writer.RenderEndTag();
            // </table></p>

            // </html>
            writer.RenderEndTag();

            writer.Flush();
            mem.Position = 0;

            string html = Encoding.UTF8.GetString(mem.ToArray());

            const string test = "<html>\r\n\t<h2 align=\"center\">\r\n\t\tTestScenario2\r\n\t</h2>\r\n\t<p>\r\n\t\t<h3 align=\"center\">\r\n\t\t\tHello World\r\n\t\t</h3>\r\n\t</p>\r\n\t<p>\r\n\t\t<table align=\"center\" border=\"0\" cellspacing=\"1\" cellpadding=\"2\" width=\"1024\">\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<th colspan=\"3\">\r\n\t\t\t\t\t<font face=\"Times New Roman\">\r\n\t\t\t\t\t\tA nice summary of things\r\n\t\t\t\t\t</font>\r\n\t\t\t\t</th>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<th align=\"left\">\r\n\t\t\t\t\t<font face=\"Times New Roman\">\r\n\t\t\t\t\t\tBegan\r\n\t\t\t\t\t</font>\r\n\t\t\t\t</th>\r\n\t\t\t\t<th colspan=\"2\" align=\"right\">\r\n\t\t\t\t\t12:00pm\r\n\t\t\t\t</th>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<th align=\"left\">\r\n\t\t\t\t\t<font face=\"Times New Roman\">\r\n\t\t\t\t\t\tEnded\r\n\t\t\t\t\t</font>\r\n\t\t\t\t</th>\r\n\t\t\t\t<th colspan=\"2\" align=\"right\">\r\n\t\t\t\t\t1:00pm\r\n\t\t\t\t</th>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<th align=\"left\">\r\n\t\t\t\t\t<font face=\"arial\">\r\n\t\t\t\t\t\tElapsed\r\n\t\t\t\t\t</font>\r\n\t\t\t\t</th>\r\n\t\t\t\t<th align=\"right\" colspan=\"2\">\r\n\t\t\t\t\t01:00:00\r\n\t\t\t\t</th>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<th align=\"left\">\r\n\t\t\t\t\t<font face=\"Times New Roman\">\r\n\t\t\t\t\t\tResult\r\n\t\t\t\t\t</font>\r\n\t\t\t\t</th>\r\n\t\t\t\t<th colspan=\"2\" align=\"right\">\r\n\t\t\t\t\tGreat Success!\r\n\t\t\t\t</th>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<th align=\"left\">\r\n\t\t\t\t\t<font face=\"Times New Roman\">\r\n\t\t\t\t\t\tA wonderful description\r\n\t\t\t\t\t</font>\r\n\t\t\t\t</th>\r\n\t\t\t\t<th colspan=\"2\" align=\"right\">\r\n\t\t\t\t\tSome text\r\n\t\t\t\t</th>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t</table>\r\n\t</p>\r\n</html>\r\n";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void CheckNotDisposed()
        {
            MemoryStream mem = new MemoryStream();
            StreamWriter sw = new StreamWriter(mem);

            using (HtmlTextWriter writer = new HtmlTextWriter(sw))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Html);
                writer.RenderEndTag();
                writer.Flush();
            }

            mem.Position = 0;
            string html = Encoding.UTF8.GetString(mem.ToArray());

            const string test = "<html />\r\n";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestScenario3()
        {
            string bodyBackgroundColor = "#E6FFFF";
            string bodyBorderColor = "#C1CDC1";
            string fontFamily = "Arial";
            string bodyFontSize = "13";
            string TestHeaderBackgroundColor = "#FFF59D";
            string bodyResultFontSize = "15";
            string TestResultStatusPass = "#66FF66";
            string TestResultStatusFail = "#FF0000";

            MemoryStream mem = new MemoryStream();
            StreamWriter sw = new StreamWriter(mem);
            HtmlTextWriter writer = new HtmlTextWriter(sw);

            // Open html
            writer.RenderBeginTag(HtmlTextWriterTag.Html);

            // <h2>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            writer.RenderBeginTag(HtmlTextWriterTag.H2);
            writer.Write("TestScenario3");
            writer.RenderEndTag();
            // </h2>

            // <p>
            writer.RenderBeginTag(HtmlTextWriterTag.P);

            // <h3>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            writer.RenderBeginTag(HtmlTextWriterTag.H3);

            writer.Write("Hello {0}", "World");

            writer.RenderEndTag();
            // </h3>
            writer.RenderEndTag();
            // </p>

            // <p>
            writer.RenderBeginTag(HtmlTextWriterTag.P);

            // Attributes to table
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, "Table");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "3");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, bodyBackgroundColor);
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, bodyBorderColor);
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "solid");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, fontFamily);
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, bodyFontSize);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");

            // Start the table
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            // Start the rows
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("HTML Sample Test Case");
            writer.RenderEndTag();
            // </td>
            writer.RenderEndTag();
            // </tr>            
            writer.RenderEndTag();
            // </table>
            writer.RenderEndTag();
            // </p>

            // Attributes to table
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, "Table");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "3");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, bodyBackgroundColor);
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, bodyBorderColor);
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "solid");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, fontFamily);
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, bodyFontSize);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");

            // Start the table
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, TestHeaderBackgroundColor);
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, bodyResultFontSize);
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "Bold");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Test Scenario");
            writer.RenderEndTag();
            // </td>

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Result");
            writer.RenderEndTag();
            // </td>

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Parameter A");
            writer.RenderEndTag();
            // </td>

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Parameter B");
            writer.RenderEndTag();
            // </td>

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Parameter C");
            writer.RenderEndTag();
            // </td>

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Parameter D");
            writer.RenderEndTag();
            // </td>

            for (var i = 1; i < 6; i++)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write("Test Scenario " + i);
                writer.RenderEndTag();
                // </td>

                if (i == 1)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, TestResultStatusPass);
                else if (i == 3)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, TestResultStatusFail);

                writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, bodyResultFontSize);
                writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "Bold");

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write("Result");
                writer.RenderEndTag();
                // </td>

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write("Sample Data " + i);
                writer.RenderEndTag();
                // </td>

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write("Test Data " + i);
                writer.RenderEndTag();
                // </td>

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write("Random Content " + i);
                writer.RenderEndTag();
                // </td>

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write("Sample Notes " + i);
                writer.RenderEndTag();
                // </td>

                writer.RenderEndTag();
                // </tr>               
            }
            writer.RenderEndTag();
            // </tr>              
            writer.RenderEndTag();
            // </table>

            // <p>
            writer.RenderBeginTag(HtmlTextWriterTag.P);

            // Attributes to table
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, "Table");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "3");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, bodyBackgroundColor);
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, bodyBorderColor);
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "solid");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontFamily, fontFamily);
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, bodyFontSize);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");

            // Start the table
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            // Start the rows
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Test Case Completed");
            writer.RenderEndTag();
            // </td>
            writer.RenderEndTag();
            // </tr>
            writer.RenderEndTag();
            // </table> 
            writer.RenderEndTag();
            // </p>
            writer.RenderEndTag();
            // </html>

            writer.Flush();
            mem.Position = 0;

            string html = Encoding.UTF8.GetString(mem.ToArray());

            const string test = "<html>\r\n\t<h2 align=\"center\">\r\n\t\tTestScenario3\r\n\t</h2>\r\n\t<p>\r\n\t\t<h3 align=\"center\">\r\n\t\t\tHello World\r\n\t\t</h3>\r\n\t</p>\r\n\t<p>\r\n\t\t<table alt=\"Table\" cellpadding=\"3\" style=\"Background-Color:#E6FFFF; Border-Color:#C1CDC1; Border-Style:solid; Font-Family:Arial; Font-Size:13; Width:100%; \">\r\n\t\t\t<tr>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tHTML Sample Test Case\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t</table>\r\n\t</p>\r\n\t<table alt=\"Table\" cellpadding=\"3\" style=\"Background-Color:#E6FFFF; Border-Color:#C1CDC1; Border-Style:solid; Font-Family:Arial; Font-Size:13; Width:100%; \">\r\n\t\t<tr style=\"Background-Color:#FFF59D; Font-Size:15; Font-Weight:Bold; \">\r\n\t\t\t<td>\r\n\t\t\t\tTest Scenario\r\n\t\t\t</td>\r\n\t\t\t<td>\r\n\t\t\t\tResult\r\n\t\t\t</td>\r\n\t\t\t<td>\r\n\t\t\t\tParameter A\r\n\t\t\t</td>\r\n\t\t\t<td>\r\n\t\t\t\tParameter B\r\n\t\t\t</td>\r\n\t\t\t<td>\r\n\t\t\t\tParameter C\r\n\t\t\t</td>\r\n\t\t\t<td>\r\n\t\t\t\tParameter D\r\n\t\t\t</td>\r\n\t\t\t<tr>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tTest Scenario 1\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"Background-Color:#66FF66; Font-Size:15; Font-Weight:Bold; \">\r\n\t\t\t\t\tResult\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tSample Data 1\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tTest Data 1\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tRandom Content 1\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tSample Notes 1\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tTest Scenario 2\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"Font-Size:15; Font-Weight:Bold; \">\r\n\t\t\t\t\tResult\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tSample Data 2\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tTest Data 2\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tRandom Content 2\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tSample Notes 2\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tTest Scenario 3\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"Background-Color:#FF0000; Font-Size:15; Font-Weight:Bold; \">\r\n\t\t\t\t\tResult\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tSample Data 3\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tTest Data 3\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tRandom Content 3\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tSample Notes 3\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tTest Scenario 4\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"Font-Size:15; Font-Weight:Bold; \">\r\n\t\t\t\t\tResult\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tSample Data 4\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tTest Data 4\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tRandom Content 4\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tSample Notes 4\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tTest Scenario 5\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"Font-Size:15; Font-Weight:Bold; \">\r\n\t\t\t\t\tResult\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tSample Data 5\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tTest Data 5\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tRandom Content 5\r\n\t\t\t\t</td>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tSample Notes 5\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t</tr>\r\n\t</table>\r\n\t<p>\r\n\t\t<table alt=\"Table\" cellpadding=\"3\" style=\"Background-Color:#E6FFFF; Border-Color:#C1CDC1; Border-Style:solid; Font-Family:Arial; Font-Size:13; Width:100%; \">\r\n\t\t\t<tr>\r\n\t\t\t\t<td>\r\n\t\t\t\t\tTest Case Completed\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t</table>\r\n\t</p>\r\n</html>\r\n";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestWriteBeginEndTag()
        {
            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw, String.Empty);

            writer.WriteBeginTag("table");
            writer.WriteBeginTag("tr");
            writer.WriteBeginTag("th"); writer.Write("RouteID"); writer.WriteEndTag("th");
            writer.WriteEndTag("tr");
            writer.WriteEndTag("table");

            string html = sw.ToString();
            Assert.AreEqual("<table<tr<thRouteID</th></tr></table>", html);
        }
    }
}
