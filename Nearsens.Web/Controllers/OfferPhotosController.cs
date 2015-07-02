using Nearsens.DataAccess;
using Nearsens.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Nearsens.Utilities;
using System.Web;

namespace Nearsens.Web.Controllers
{
    public class OfferPhotosController : ApiController
    {
        SqlOffersRepository repository = new SqlOffersRepository();

        [Authorize]
        public void Delete([FromBody] DeletePhotosCommand deletePhotos)
        {
            repository.DeletePhotos(deletePhotos.PhotosId);
            deletePhotos.DeleteFromFileSystem(HttpContext.Current.Server);
        }

        [Authorize]
        [HttpGet]
        public IEnumerable<OfferPhoto> GetPhotosOfTheOffer(long idOffer)
        {
            return repository.GetOfferPhotos(idOffer);
        }
    }
}