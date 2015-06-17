using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nearsens.Models
{
    public class GetNearestPlacesQuery
    {
        public long Id { get; set; }
        public string MainCategory { get; set; }
        public string Subcategory { get; set; }
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Icon { get; set; }
        public OfferForNearestPlacesQuery FirstOffer { get; set; }
    }
}
