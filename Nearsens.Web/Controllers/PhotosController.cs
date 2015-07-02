using Nearsens.DataAccess;
using Nearsens.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Nearsens.Web.Controllers
{
    public class PhotosController : ApiController
    {
        SqlPlacesRepository repository = new SqlPlacesRepository();

        [Authorize]
        public void Delete(int[] listaId)
        {
            repository.DeletePhotos(listaId);
        }

        [Authorize]
        [HttpGet]
        public IEnumerable<Photo> GetPhotosOfThePlace(long idPlace)
        {
             return repository.GetPlacePhotos(idPlace);
        }
    }
}
