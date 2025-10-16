using System.ComponentModel.DataAnnotations;
using CsvHelper;
using CsvHelper.Configuration;

namespace Winzer.Impl.CsvHelper;
public class TruncatingStringConverter : DelimiterRemovingStringConverter
{
    public TruncatingStringConverter(string delimiter = ",")
        :base(delimiter){ }

    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        var str = base.ConvertToString(value, row, memberMapData);

        var x = memberMapData.Member.GetCustomAttributes(typeof(StringLengthAttribute), true)
                                    .SingleOrDefault() as StringLengthAttribute;
        if (x != null)
        {
            return Truncate(str, x.MaximumLength);
        }
        return str;
    }

    private string Truncate(string value, int maxLength)
    {
        return value.Length > maxLength
            ? value.Substring(0, maxLength)
            : value;
    }
}
