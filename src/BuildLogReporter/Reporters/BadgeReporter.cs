using System.Globalization;
using System.Text.RegularExpressions;
using BuildLogReporter.Processors;
using SixLabors.Fonts;

namespace BuildLogReporter.Reporters
{
    public sealed class BadgeReporter : Reporter
    {
        private const string PlasticBadgeTemplate = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""{0}"" height=""20"">
  <linearGradient id=""b"" x2=""0"" y2=""100%"">
    <stop offset=""0"" stop-color=""#FFFFFF"" stop-opacity=""0.7"" />
    <stop offset=""0.1"" stop-color=""#AAAAAA"" stop-opacity=""0.1"" />
    <stop offset=""0.9"" stop-opacity=""0.3"" />
    <stop offset=""1"" stop-opacity=""0.5"" />
  </linearGradient>
  <mask id=""a"">
    <rect width=""{0}"" height=""20"" rx=""4"" fill=""#FFFFFF"" />
  </mask>
  <g mask=""url(#a)""><path fill=""#555555"" d=""M0 0h{1}v20H0z"" />
    <path fill=""{7}"" d=""M{1} 0h{2}v20H{1}z"" />
    <path fill=""url(#b)"" d=""M0 0h{0}v20H0z"" />
  </g>
  <g fill=""#FFFFFF"" text-anchor=""middle"" font-family=""Verdana,Geneva,sans-serif"" font-size=""11"">
    <text x=""{3}"" y=""15"" fill=""#010101"" fill-opacity=""0.3"">{5}</text>
    <text x=""{3}"" y=""14"">{5}</text>
    <text x=""{4}"" y=""15"" fill=""#010101"" fill-opacity=""0.3"">{6}</text>
    <text x=""{4}"" y=""14"">{6}</text>
  </g>
</svg>";

        private const string Green = "#44CC11";

        private const string Yellow = "#DFB317";

        private const string Red = "#E05D44";

        private const string LeftSideText = "Build";

        private const string ExtensionValue = "svg";

        private readonly string[] _possibleFonts = new string[] { "Verdana", "Geneva", "sans-serif" };

        public override string Extension => ExtensionValue;

        public override string GetReportAsString(ProcessedLogResult processedLogResult)
        {
            string rightSideColor;
            if (processedLogResult.ErrorCount > 0)
            {
                rightSideColor = Red;
            }
            else if (processedLogResult.WarningCount > 0)
            {
                rightSideColor = Yellow;
            }
            else
            {
                rightSideColor = Green;
            }

            string errorText = processedLogResult.ErrorCount == 1 ? "error" : "errors";
            string warningsText = processedLogResult.WarningCount == 1 ? "warning" : "warnings";
            string errorsAndWarningsText = $"{processedLogResult.ErrorCount} {errorText}, {processedLogResult.WarningCount} {warningsText}";

            Font? font = null;
            foreach (string possibleFont in _possibleFonts)
            {
                try
                {
                    font = SystemFonts.CreateFont(possibleFont, 11, FontStyle.Regular);
                    break;
                }
                catch (FontFamilyNotFoundException)
                {
                }
            }

            if (font is null)
            {
                throw new InvalidOperationException("Could not initialize font.");
            }

            var buildRectangle = TextMeasurer.Measure(LeftSideText, new TextOptions(font));
            var errorsAndWarningsRectangle = TextMeasurer.Measure(errorsAndWarningsText, new TextOptions(font));

            var buildRectangleWidth = buildRectangle.Width + 3;
            var errorsAndWarningsRectangleWidth = errorsAndWarningsRectangle.Width + 3;

            var badge = string.Format(
                CultureInfo.InvariantCulture,
                PlasticBadgeTemplate,
                buildRectangleWidth + errorsAndWarningsRectangleWidth + 1,
                buildRectangleWidth + 1,
                errorsAndWarningsRectangleWidth + 1,
                (buildRectangleWidth / 2) + 1,
                buildRectangleWidth + (errorsAndWarningsRectangleWidth / 2) + 1,
                LeftSideText,
                errorsAndWarningsText,
                rightSideColor);

            return Regex.Replace(Regex.Replace(badge, @"\t|\n|\r", string.Empty), @">\s+<", @"><");
        }
    }
}
