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

            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw);

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
            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw);

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

            string html = sw.ToString();

            const string test = "<span onclick=\"alert(&#39;Hello&#39;);\" CustomAttribute=\"CustomAttributeValue\">\r\n\tHello\r\n\t<img alt=\"Encoding, &quot;Required&quot;\" myattribute=\"No &quot;encoding &quot; required\" />\r\n\t<MyTag>\r\n\t\tContents of MyTag\r\n\t</MyTag>\r\n\t<img alt=\"A custom image.\"></img>\r\n</span>";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestScenario1()
        {
            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw);

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

            string html = sw.ToString();

            const string test = "<html>\r\n\t<center>\r\n\t\t<p><h2>\r\n\t\t\t1\r\n\t\t</h2><img src=\"/a\" width=\"640\" height=\"480\" /><img src=\"/b\" width=\"640\" height=\"480\" /><img src=\"/c\" width=\"640\" height=\"480\" /></p>\r\n\t\t<p><h2>\r\n\t\t\t2\r\n\t\t</h2><img src=\"/a\" width=\"640\" height=\"480\" /><img src=\"/b\" width=\"640\" height=\"480\" /><img src=\"/c\" width=\"640\" height=\"480\" /></p>\r\n\t\t<p><h2>\r\n\t\t\t3\r\n\t\t</h2><img src=\"/a\" width=\"640\" height=\"480\" /><img src=\"/b\" width=\"640\" height=\"480\" /><img src=\"/c\" width=\"640\" height=\"480\" /></p>\r\n\t\t<p><h2>\r\n\t\t\t4\r\n\t\t</h2><img src=\"/a\" width=\"640\" height=\"480\" /><img src=\"/b\" width=\"640\" height=\"480\" /><img src=\"/c\" width=\"640\" height=\"480\" /></p>\r\n\t\t<p><h2>\r\n\t\t\t5\r\n\t\t</h2><img src=\"/a\" width=\"640\" height=\"480\" /><img src=\"/b\" width=\"640\" height=\"480\" /><img src=\"/c\" width=\"640\" height=\"480\" /></p>\r\n\r\n\t</center>\r\n</html>";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestScenario2()
        {
            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw);

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

            string html = sw.ToString();

            const string test = "<html>\r\n\t<h2 align=\"center\">\r\n\t\tTestScenario2\r\n\t</h2><p><h3 align=\"center\">\r\n\t\tHello World\r\n\t</h3></p><p><table align=\"center\" border=\"0\" cellspacing=\"1\" cellpadding=\"2\" width=\"1024\">\r\n\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t<th colspan=\"3\"><font face=\"Times New Roman\">A nice summary of things</font></th>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<th align=\"left\"><font face=\"Times New Roman\">Began</font></th><th colspan=\"2\" align=\"right\">12:00pm</th>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<th align=\"left\"><font face=\"Times New Roman\">Ended</font></th><th colspan=\"2\" align=\"right\">1:00pm</th>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<th align=\"left\"><font face=\"arial\">Elapsed</font></th><th align=\"right\" colspan=\"2\">01:00:00</th>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<th align=\"left\"><font face=\"Times New Roman\">Result</font></th><th colspan=\"2\" align=\"right\">Great Success!</th>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<th align=\"left\"><font face=\"Times New Roman\">A wonderful description</font></th><th colspan=\"2\" align=\"right\">Some text</th>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<td>/a/b/c</td><td align=\"right\">26 files</td><td align=\"right\">200GB</td>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<td>/a/b/c</td><td align=\"right\">26 files</td><td align=\"right\">200GB</td>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<td>/a/b/c</td><td align=\"right\">26 files</td><td align=\"right\">200GB</td>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<td>/a/b/c</td><td align=\"right\">26 files</td><td align=\"right\">200GB</td>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<td>/a/b/c</td><td align=\"right\">26 files</td><td align=\"right\">200GB</td>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<td>/a/b/c</td><td align=\"right\">26 files</td><td align=\"right\">200GB</td>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<td>/a/b/c</td><td align=\"right\">26 files</td><td align=\"right\">200GB</td>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<td>/a/b/c</td><td align=\"right\">26 files</td><td align=\"right\">200GB</td>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<td>/a/b/c</td><td align=\"right\">26 files</td><td align=\"right\">200GB</td>\r\n\t\t</tr><tr bgcolor=\"#FFFF00\">\r\n\t\t\t<td>/a/b/c</td><td align=\"right\">26 files</td><td align=\"right\">200GB</td>\r\n\t\t</tr>\r\n\t</table></p>\r\n</html>";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void CheckStreamNotDisposed()
        {
            MemoryStream mem = new();
            StreamWriter sw = new(mem);

            using (HtmlTextWriter writer = new(sw))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Html);
                writer.RenderEndTag();
                writer.Flush();
            }
            mem.Position = 0;
            string html = Encoding.UTF8.GetString(mem.ToArray());

            const string test = "<html>\r\n\r\n</html>";
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

            StringWriter sw = new();
            HtmlTextWriter writer = new(sw);

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

            string html = sw.ToString();

            const string test = "<html>\r\n\t<h2 align=\"center\">\r\n\t\tTestScenario3\r\n\t</h2><p><h3 align=\"center\">\r\n\t\tHello World\r\n\t</h3></p><p><table alt=\"Table\" cellpadding=\"3\" style=\"background-color:#E6FFFF;border-color:#C1CDC1;border-style:solid;font-family:Arial;font-size:13;width:100%;\">\r\n\t\t<tr>\r\n\t\t\t<td>HTML Sample Test Case</td>\r\n\t\t</tr>\r\n\t</table></p><table alt=\"Table\" cellpadding=\"3\" style=\"background-color:#E6FFFF;border-color:#C1CDC1;border-style:solid;font-family:Arial;font-size:13;width:100%;\">\r\n\t\t<tr style=\"background-color:#FFF59D;font-size:15;font-weight:Bold;\">\r\n\t\t\t<td>Test Scenario</td><td>Result</td><td>Parameter A</td><td>Parameter B</td><td>Parameter C</td><td>Parameter D</td><tr>\r\n\t\t\t\t<td>Test Scenario 1</td><td style=\"background-color:#66FF66;font-size:15;font-weight:Bold;\">Result</td><td>Sample Data 1</td><td>Test Data 1</td><td>Random Content 1</td><td>Sample Notes 1</td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td>Test Scenario 2</td><td style=\"font-size:15;font-weight:Bold;\">Result</td><td>Sample Data 2</td><td>Test Data 2</td><td>Random Content 2</td><td>Sample Notes 2</td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td>Test Scenario 3</td><td style=\"background-color:#FF0000;font-size:15;font-weight:Bold;\">Result</td><td>Sample Data 3</td><td>Test Data 3</td><td>Random Content 3</td><td>Sample Notes 3</td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td>Test Scenario 4</td><td style=\"font-size:15;font-weight:Bold;\">Result</td><td>Sample Data 4</td><td>Test Data 4</td><td>Random Content 4</td><td>Sample Notes 4</td>\r\n\t\t\t</tr><tr>\r\n\t\t\t\t<td>Test Scenario 5</td><td style=\"font-size:15;font-weight:Bold;\">Result</td><td>Sample Data 5</td><td>Test Data 5</td><td>Random Content 5</td><td>Sample Notes 5</td>\r\n\t\t\t</tr>\r\n\t\t</tr>\r\n\t</table><p><table alt=\"Table\" cellpadding=\"3\" style=\"background-color:#E6FFFF;border-color:#C1CDC1;border-style:solid;font-family:Arial;font-size:13;width:100%;\">\r\n\t\t<tr>\r\n\t\t\t<td>Test Case Completed</td>\r\n\t\t</tr>\r\n\t</table></p>\r\n</html>";
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

        [TestMethod]
        public void TestEncodedUrl()
        {
            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw, String.Empty);

            writer.WriteEncodedUrl("http://localhost/SampleFolder/Sample + File.txt");

            string html = sw.ToString();

            const string test = "http://localhost/SampleFolder/Sample%20%2b%20File.txt";
            const string test2 = "http%3A%2F%2Flocalhost%2FSampleFolder%2FSample%20%2B%20File.txt";

            Assert.IsTrue(html == test || html == test2);
        }

        [TestMethod]
        public void TestEncodedUrlQueryString()
        {
            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw, String.Empty);

            writer.WriteEncodedUrl("http://localhost/SampleFolder/Sample + File.txt?v=1");

            string html = sw.ToString();

            const string test = "http://localhost/SampleFolder/Sample%20%2b%20File.txt?v=1";
            const string test2 = "http%3A%2F%2Flocalhost%2FSampleFolder%2FSample%20%2B%20File.txt?v=1";

            Assert.IsTrue(html == test || html == test2);
        }

        [TestMethod]
        public void TestWriteEmptyTags()
        {
            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw);

            writer.RenderBeginTag("textarea");
            writer.Write(String.Empty);
            writer.RenderEndTag();

            writer.RenderBeginTag("Foo");
            writer.Write(String.Empty);
            writer.RenderEndTag();

            writer.RenderBeginTag("TextArea");
            writer.Write(String.Empty);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Textarea);
            writer.Write(String.Empty);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write(String.Empty);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();

            string html = sw.ToString();

            const string test = "<textarea></textarea><Foo>\r\n\t\r\n</Foo><textarea></textarea><textarea></textarea><div>\r\n\t\r\n</div><div>\r\n\r\n</div><img />";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestWriteStyleAttribute()
        {
            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw);

            writer.AddStyleAttribute(HtmlTextWriterStyle.Color, "Red");
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "16");
            writer.AddStyleAttribute("Font-Family", "Arial");
            writer.AddStyleAttribute("CustomStyle", "CustomStyleValue");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.WriteStyleAttribute("Font-size", "23");
            writer.Write("Hello");
            writer.RenderEndTag();

            string html = sw.ToString();

            const string test = "<span style=\"color:Red;font-size:16;Font-Family:Arial;CustomStyle:CustomStyleValue;\">Font-size:23;Hello</span>";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestLongAttributeValue()
        {
            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw, String.Empty);

            writer.AddAttribute("data-json", """{"style-color":"#ff0000", "style-background":"#000000", "style-width":1024, "style-height": 768, "style-top": 0, "style-left": 0, "style-right": 0, "style-bottom": 0, "style-position": "absolute", "style-display": "block"}""");
            writer.RenderBeginTag("div");
            writer.RenderEndTag();

            string html = sw.ToString();

            const string test = "<div data-json=\"{&quot;style-color&quot;:&quot;#ff0000&quot;, &quot;style-background&quot;:&quot;#000000&quot;, &quot;style-width&quot;:1024, &quot;style-height&quot;: 768, &quot;style-top&quot;: 0, &quot;style-left&quot;: 0, &quot;style-right&quot;: 0, &quot;style-bottom&quot;: 0, &quot;style-position&quot;: &quot;absolute&quot;, &quot;style-display&quot;: &quot;block&quot;}\">\r\n\r\n</div>";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestDefaultNewLine()
        {
            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw, String.Empty);

            writer.RenderBeginTag(HtmlTextWriterTag.Html);
            writer.RenderBeginTag(HtmlTextWriterTag.Body);
            writer.Write("Hello World");
            writer.RenderEndTag();
            writer.RenderEndTag();

            string html = sw.ToString();

            const string test = "<html>\r\n<body>\r\nHello World\r\n</body>\r\n</html>";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestEmptyNewLine()
        {
            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw, String.Empty) { NewLine = String.Empty };

            writer.RenderBeginTag(HtmlTextWriterTag.Html);
            writer.RenderBeginTag(HtmlTextWriterTag.Body);
            writer.Write("Hello World");
            writer.RenderEndTag();
            writer.RenderEndTag();

            string html = sw.ToString();

            const string test = "<html><body>Hello World</body></html>";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestRenderBeginTagBehaviors()
        {
            for (HtmlTextWriterTag tag = HtmlTextWriterTag.A; tag <= HtmlTextWriterTag.Xml; tag++)
                TestRenderBeginTagBehaviors(tag.ToString().ToLower(), tag.ToString());

            TestRenderBeginTagBehaviors("Random", "Random");
        }

        static void TestRenderBeginTagBehaviors(string name, string tag)
        {
            using StringWriter sw = new();
            using HtmlTextWriter writer = new(sw);

            writer.RenderBeginTag(tag);

            string html = sw.ToString();

            string test1 = $"<{name}>";
            string test2 = $"<{name} />";
            string test3 = $"<{name}>\r\n";

            string beginTagBehavior;
            if (html == test1)
                beginTagBehavior = "OpenTag";
            else if (html == test2)
                beginTagBehavior = "SelfClose";
            else if (html == test3)
                beginTagBehavior = "OpenTagWithLineBreak";
            else
                beginTagBehavior = "??";

            bool indent = writer.Indent > 0;
            string indentBehavior = indent ? "Indent" : "NoIndent";

            Debug.WriteLine($"\t{{ HtmlTextWriterTag.{tag}, new(\"{name}\", BeginTagBehavior.{beginTagBehavior}, BeginTagIndentBehavior.{indentBehavior}) }},");
        }
    }
}
