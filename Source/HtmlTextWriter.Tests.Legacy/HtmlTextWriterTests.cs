// (c) 2024 Max Feingold

using System.IO;
using System;
using System.Web.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HtmlTextWriter.Tests.Legacy
{
    [TestClass]
    public class HtmlTextWriterTests
    {
        [TestMethod]
        public void TestWriteBeginEndTag()
        {
            using (StringWriter sw = new StringWriter())
            using (var writer = new System.Web.UI.HtmlTextWriter(sw, String.Empty))
            {
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
}
