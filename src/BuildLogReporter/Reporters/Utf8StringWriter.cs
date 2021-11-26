﻿using System.Text;

namespace BuildLogReporter.Reporters
{
    public sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
