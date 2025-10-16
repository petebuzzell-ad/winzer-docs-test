using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Util
{
    public static class Slugifier
    {
        public static string? Slugify(string? name)
        {
            if (name == null) return null;
            var cleaned = Regex.Replace(name.Trim().ToLower(), @"[^a-z0-9]+", "-", RegexOptions.Compiled);
            return cleaned;
        }
    }
}
