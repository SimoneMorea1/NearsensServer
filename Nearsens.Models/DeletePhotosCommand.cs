using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nearsens.Models
{
    public class DeletePhotosCommand
    {
        public long[] PhotosId { get; set; }
        public string[] PhotosPath { get; set; }
        public long PlaceId { get; set; }
    }
}
