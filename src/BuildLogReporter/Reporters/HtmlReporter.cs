using System.Globalization;
using System.Text;
using BuildLogReporter.Processors;

namespace BuildLogReporter.Reporters
{
    public sealed class HtmlReporter : Reporter
    {
        private const string HtmlTemplate = @"<html>
  <head>
    <style>
      body {{
        font-family: 'Open Sans', sans-serif;
        width: 95%;
        margin: 0 auto;
        padding: 1em 0 2em;
        color: #000000;
      }}

      #searchInput {{
        width: 95%;
        display: block;
        margin-right: auto;
        margin-left: auto;
        margin-bottom: 6px;
        font-size: 16px;
        padding: 6px 10px 6px 20px;
        background-color: #000000;
        color: #FFFFFF;
        border: 1px solid #000000;
      }}

      ::placeholder {{
        color: #FFFFFF;
      }}

      table {{
        width: 95%;
        border-collapse: collapse;
        margin-left:auto;
        margin-right:auto;
      }}

      tr:nth-child(odd) {{
        background-color: #FFFFFF;
      }}

      tr:nth-child(even) {{
        background-color: #DCDCDC;
      }}

      table, th, td {{
        border: 1px solid #000000;
        padding: 3px;
      }}

      th {{
        background-color: #000000;
        color: #FFFFFF;
        cursor: pointer;
      }}

      p {{
        width: 95%;
        margin-left:auto;
        margin-right:auto;
        color: #000000;
      }}
    </style>
    <title>{0}</title>
  </head>
  <body>
    <script>
      function searchTable() {{
        let input = document.getElementById(""searchInput"");
        let filter = input.value.toUpperCase();
        let table = document.getElementById(""errorsAndWarningsTable"");
        let tr = table.getElementsByTagName(""tr"");
        let columnLength = table.rows[0].cells.length;
        for (let i = 1; i < tr.length; i++) {{
          let found = false;
          for (let j = 0; j < columnLength; j++) {{
            let td = tr[i].getElementsByTagName(""td"")[j];
            if (td) {{
              txtValue = td.textContent || td.innerText;
              if (txtValue.toUpperCase().indexOf(filter) > -1) {{
                found = true;
                continue;
              }}
            }}
          }}

          if (found) {{
            tr[i].style.display = """";
          }} else {{
            tr[i].style.display = ""none"";
          }}
        }}
      }}

      function sortTable(columnIndex, isNumeric) {{
        let table = document.getElementById(""errorsAndWarningsTable"");
        let switching = true;
        let dir = ""asc"";
        let shouldSwitch = false;
        let switchCount = 0;
        let i = 0;

        while (switching) {{
          switching = false;
          let rows = table.rows;
          for (i = 1; i<(rows.length - 1); i++) {{
            shouldSwitch = false;
            let x = rows[i].getElementsByTagName(""TD"")[columnIndex];
            let y = rows[i + 1].getElementsByTagName(""TD"")[columnIndex];
            if (dir == ""asc"") {{
              if (isNumeric) {{
                if (Number(x.innerHTML) > Number(y.innerHTML)) {{
                  shouldSwitch = true;
                  break;
                }}
              }} else {{
                if (x.innerHTML.toLowerCase() > y.innerHTML.toLowerCase()) {{
                  shouldSwitch = true;
                  break;
                }}
              }}
            }} else if (dir == ""desc"") {{
                if (isNumeric) {{
                  if (Number(x.innerHTML) < Number(y.innerHTML)) {{
                    shouldSwitch = true;
                    break;
                  }}
              }} else {{
                if (x.innerHTML.toLowerCase() < y.innerHTML.toLowerCase()) {{
                  shouldSwitch = true;
                  break;
                }}
              }}
            }}
          }}

          if (shouldSwitch) {{
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;
            switchCount++;
          }} else {{
            if (switchCount == 0 && dir == ""asc"") {{
              dir = ""desc"";
              switching = true;
            }}
          }}
        }}
      }}
    </script>
    <p>Build report for '{1}'.</br>Errors:&nbsp;{2},&nbsp;Warnings:&nbsp;{3}</p>
    <input type=""text"" id=""searchInput"" onkeyup=""searchTable()"" placeholder=""Search table..."">
    <table id=""errorsAndWarningsTable"">
      <th style=""width: 5%"" onclick=""sortTable(0, false)"">Type</th><th style=""width: 10%"" onclick=""sortTable(1, false)"">Code</th><th style=""width: 40%"" onclick=""sortTable(2, false)"">Description</th><th style=""width: 40%"" onclick=""sortTable(3, false)"">File</th><th style=""width: 5%"" onclick=""sortTable(4, true)"">Line</th>
{4}
    </table>
  </body>
</html>";

        private const string ErrorSymbol = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""20"" height=""20"" viewBox=""0 0 24 24""><path d=""M16.142 2l5.858 5.858v8.284l-5.858 5.858h-8.284l-5.858-5.858v-8.284l5.858-5.858h8.284zm.829-2h-9.942l-7.029 7.029v9.941l7.029 7.03h9.941l7.03-7.029v-9.942l-7.029-7.029zm-8.482 16.992l3.518-3.568 3.554 3.521 1.431-1.43-3.566-3.523 3.535-3.568-1.431-1.432-3.539 3.583-3.581-3.457-1.418 1.418 3.585 3.473-3.507 3.566 1.419 1.417z"" fill=""#EE4B2B"" /></svg>";

        private const string WarningSymbol = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""20"" height=""20"" viewBox=""0 0 24 24""><path d=""M12 5.177l8.631 15.823h-17.262l8.631-15.823zm0-4.177l-12 22h24l-12-22zm-1 9h2v6h-2v-6zm1 9.75c-.689 0-1.25-.56-1.25-1.25s.561-1.25 1.25-1.25 1.25.56 1.25 1.25-.561 1.25-1.25 1.25z"" fill=""#FFE900"" /></svg>";

        private const string ExtensionValue = "htm";

        private readonly string _logPath;

        public override string Extension => ExtensionValue;

        public override string GetReportAsString(ProcessedLogResult processedLogResult)
        {
            var tableRowsStringBuilder = new StringBuilder();
            foreach (var logEntry in processedLogResult.LogEntries)
            {
                tableRowsStringBuilder.Append("      <tr><td style=\"text-align:center;\">")
                    .Append(logEntry.Type == LogEntryType.Error ? ErrorSymbol : WarningSymbol)
                    .Append("</td><td>")
                    .Append(logEntry.Code)
                    .Append("</td><td>")
                    .Append(logEntry.Message)
                    .Append("</td><td>")
                    .Append(logEntry.FilePath)
                    .Append("</td><td>")
                    .Append(logEntry.LineNumber)
                    .AppendLine("</td></tr>");
            }

            if (tableRowsStringBuilder.Length >= Environment.NewLine.Length)
            {
                tableRowsStringBuilder.Length -= Environment.NewLine.Length;
            }

            var htmlPage = string.Format(
                CultureInfo.InvariantCulture,
                HtmlTemplate,
                $"Report for '{Path.GetFileName(_logPath)}'",
                _logPath,
                processedLogResult.ErrorCount.ToString(CultureInfo.InvariantCulture),
                processedLogResult.WarningCount.ToString(CultureInfo.InvariantCulture),
                tableRowsStringBuilder.ToString());

            return htmlPage;
        }

        public HtmlReporter(string logPath)
        {
            _logPath = logPath;
        }
    }
}
