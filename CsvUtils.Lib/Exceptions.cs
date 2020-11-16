using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CsvUtils.Lib
{
    public class CsvTypeException : Exception
    {
        public CsvTypeException(Type destType, string itemValString, int lineNumber)
            : base($"Failed to parse {itemValString} as type {destType.ToString()} on line {lineNumber}")
        {
        }
        
        public CsvTypeException(Type destType, string itemValString)
            : base($"Failed to parse {itemValString} as type {destType.ToString()}")
        {
        }
    }

    public class CsvColumnNumberMismatchException : Exception
    {
        public CsvColumnNumberMismatchException(IList<int> lines)
            : base($"There was a mismatch in the number of columns on lines {string.Join(", ", lines)}")
        {
        }
    }

    public class CsvUnmappedLabelsException : Exception
    {
        public CsvUnmappedLabelsException(IList<string> unmappedLabels)
            : base(
                $"The following labels do not map to properties in the destination object: {string.Join(", ", unmappedLabels)}")
        {
        }
    }

    public class CsvNoLinesException : Exception
    {
    }
}