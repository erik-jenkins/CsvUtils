using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CsvUtils.Lib
{
    public class CsvReader : IDisposable
    {
        /// <summary>
        /// Given a valid path to a CSV file, reads the file into a list
        /// of the given destination object.
        /// </summary>
        public List<TDest> ReadCsv<TDest>(string filepath) where TDest : new()
        {
            var lines = ParseLines(filepath);
            ValidateCsvLines(lines);

            return MapLinesToDestination<TDest>(lines);
        }

        /// <summary>
        /// Given a path to a CSV file, returns a list of lists of strings.
        /// The outer list is a list of rows, and each inner list
        /// is a list of columns for a particular row.
        /// </summary>
        private List<List<string>> ParseLines(string filepath)
        {
            using var reader = new StreamReader(filepath);
            var result = new List<List<string>>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null)
                    continue;

                var lineItems = GetLineItems(line);
                result.Add(lineItems);
            }

            return result;
        }

        /// <summary>
        /// Given a line of a CSV, splits the line into a list
        /// of strings that have been trimmed of whitespace on
        /// either end.
        /// </summary>
        private List<string> GetLineItems(string csvLine)
        {
            return csvLine
                .Split(",")
                .Select(col => col.Trim())
                .ToList();
        }

        /// <summary>
        /// Checks the parsed list of lists of strings for common issues,
        /// such as no rows, different columns of rows, etc.
        /// </summary>
        private void ValidateCsvLines(List<List<string>> csvLines)
        {
            if (csvLines.Count == 0)
                throw new CsvNoLinesException();

            var columnCount = csvLines.First().Count;
            var incorrectColumnNumberLines = csvLines
                .Select((row, index) =>
                {
                    if (row.Count != columnCount)
                        return index + 1; // add 1 to account for header row

                    return -1;
                })
                .Where(lineNumber => lineNumber > 0)
                .ToList();

            if (incorrectColumnNumberLines.Count > 0)
                throw new CsvColumnNumberMismatchException(incorrectColumnNumberLines);
        }

        /// <summary>
        /// Given a list of unparsed CSV lines, map those lines to a list of the destination
        /// type TDest.
        /// </summary>
        private List<TDest> MapLinesToDestination<TDest>(List<List<string>> unmappedLines) where TDest : new()
        {
            var headerLabels = unmappedLines.First();
            return unmappedLines
                .Skip(1)
                .Select(line => MapLineToDestination<TDest>(headerLabels, line))
                .ToList();
        }

        /// <summary>
        /// Given a list of strings and a list of header labels, map the
        /// strings to the properties of TDest according to the names in the headerLabels.
        /// </summary>
        private TDest MapLineToDestination<TDest>(List<string> headerLabels, List<string> unmappedLine)
            where TDest : new()
        {
            var propertyInfos = typeof(TDest).GetProperties();
            var indexPropertyInfoMap = MapIndexToPropertyInfo(headerLabels, propertyInfos);
            var dest = new TDest();

            for (var index = 0; index < unmappedLine.Count; index++)
            {
                var unmappedItem = unmappedLine[index];

                // we know propertyInfo will be defined here because we checked earlier that
                // all rows have the same number of columns
                var propertyInfo = indexPropertyInfoMap[index];
                SetPropertyValue(dest, propertyInfo, unmappedItem);
            }

            return dest;
        }

        /// <summary>
        /// Maps the column index of each header label in the CSV to a property in the
        /// destination object.
        /// </summary>
        private Dictionary<int, PropertyInfo> MapIndexToPropertyInfo(
            List<string> headerLabels,
            PropertyInfo[] propertyInfos)
        {
            var indexLabelPropertyInfoTuples = headerLabels
                .Select((label, index) =>
                    (index, label, propertyInfos.FirstOrDefault(propertyInfo => propertyInfo.Name.ToLower() == TransformCsvLabel(label))))
                .ToArray();

            var unmappedLabels = indexLabelPropertyInfoTuples
                .Where(tuple => tuple.Item3 == null)
                .Select(tuple => tuple.Item2)
                .ToList();

            if (unmappedLabels.Count > 0)
                throw new CsvUnmappedLabelsException(unmappedLabels);

            return indexLabelPropertyInfoTuples
                .Where(tuple => tuple.Item3 != null)
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item3);
        }

        /// <summary>
        /// Given a header label from the CSV, transforms it to simplify matching
        /// the header label with the destination type property.
        /// </summary>
        private string TransformCsvLabel(string label)
        {
            return label.Replace("_", "").ToLower();
        }

        private void SetPropertyValue(object dest, PropertyInfo propertyInfo, string value)
        {
            var propertyType = propertyInfo.PropertyType;
            var typeCode = Type.GetTypeCode(propertyInfo.PropertyType);
            object valueTyped = null;

            try
            {
                switch (typeCode)
                {
                    case TypeCode.String:
                        valueTyped = value;
                        break;

                    case TypeCode.Int16:
                        valueTyped = short.Parse(value);
                        break;

                    case TypeCode.Int32:
                        if (propertyType.IsEnum)
                        {
                            valueTyped = Enum.Parse(propertyType, value);
                            break;
                        }
                        
                        valueTyped = int.Parse(value);
                        break;
                    
                    case TypeCode.Int64:
                        valueTyped = double.Parse(value);
                        break;
                    
                    case TypeCode.Single:
                        valueTyped = float.Parse(value);
                        break;
                    
                    case TypeCode.Double:
                        valueTyped = double.Parse(value);
                        break;
                }
            }
            catch (Exception e)
            {
                throw new CsvTypeException(propertyType, value);
            }

            if (valueTyped == null)
            {
                throw new CsvTypeException(propertyType, value);
            }
            
            propertyInfo.SetValue(dest, valueTyped);
        }

        public void Dispose()
        {
        }
    }
}