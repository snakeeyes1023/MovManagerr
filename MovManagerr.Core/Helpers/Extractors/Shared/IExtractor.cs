using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Helpers.Extractors.Shared
{
    public interface IExtractor
    {
        IExtractionResult ExtractFromFileName(string fileName);
    }
}
