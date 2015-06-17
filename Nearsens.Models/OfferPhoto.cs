using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nearsens.Models
{
    public class OfferPhoto
    {
        public long Id { get; set; }
        public long IdOffer{ get; set; }
        public string Path { get; set; }
    }
}
