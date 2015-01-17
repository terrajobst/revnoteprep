using System;

namespace ReviewNotesPreparationTool
{
    internal sealed class ReviewSettings
    {
        public string Organization { get; set; }
        public string Repository { get; set; }
        public string Label { get; set; }
        public string OutputFileName { get; set; }
    }
}