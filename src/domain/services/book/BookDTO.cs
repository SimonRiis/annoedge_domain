using domain.book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.services.book
{
    public record BookDTO
    {
        public required long Id { get; init; }
        public required List<BookEntryDTO> Entries { get; init; }
    }
}
