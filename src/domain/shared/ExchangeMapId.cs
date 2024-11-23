using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.shared
{
    public class ExchangeMapId
    {
        public required Guid InternalId { get; init; }
        public required string ExternalId { get; init; }

        public override string ToString()
        {
            return InternalId.ToString();
        }
    }
}
