using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nearsens.Models;

namespace Nearsens.Web.Models
{
    public class ModelList
    {
        public IEnumerable<GetPlacesByUserIdQuery> places;
        public GetPlaceQuery place;
        public IEnumerable<GetOffersByPlaceIdQuery> offersByPlace;
        public GetOfferQuery offerDetail;
        public IEnumerable<string> subcategories;
    }
}
