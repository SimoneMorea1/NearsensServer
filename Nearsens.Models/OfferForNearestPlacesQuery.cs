using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nearsens.Models
{
    public class OfferForNearestPlacesQuery
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string MainPhoto { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
    }
}
