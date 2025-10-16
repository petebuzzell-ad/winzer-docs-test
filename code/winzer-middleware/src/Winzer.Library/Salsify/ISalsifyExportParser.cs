using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winzer.Library.Salsify
{
    public interface ISalsifyExportParser
    {
        public IEnumerable<SalsifyProduct> ParseSalsifyJsonExport(string json);

        public Task<IEnumerable<SalsifyProduct>> ParseSalsifyJsonExport(Uri exportUri);
    }
}
