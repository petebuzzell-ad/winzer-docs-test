using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Winzer.Impl.CsvHelper;
public class DelimiterRemovingStringConverter : StringConverter
{
    public string Delimiter { get; set; }

    public DelimiterRemovingStringConverter(string delimiter = ",")
    {
        Delimiter = delimiter;
    }

    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        var str = base.ConvertToString(value, row, memberMapData);

        return Sanitize(str);
    }

    private string Sanitize(string value)
    {
        if (String.IsNullOrEmpty(value))
            return value;

        var escapedSymbol = Regex.Escape(Delimiter);
        Regex regex = new Regex(@"[" + escapedSymbol + "]" );
        return regex.Replace(value, "");
    }
}
