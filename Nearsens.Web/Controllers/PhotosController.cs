using Nearsens.DataAccess;
using Nearsens.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web.Http;
using System.IO;
using Nearsens.Models;
using Nearsens.Utilities;

namespace Nearsens.Web.Controllers
{
    public class PhotosController : ApiController
    {
        SqlPlacesRepository repository = new SqlPlacesRepository();

        [Authorize]
        public void Delete([FromBody]DeletePhotosCommand deletePhotos)
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            repository.DeletePhotos(deletePhotos.PhotosId);
            deletePhotos.DeleteFromFileSystem(HttpContext.Current.Server);
        }

        [Authorize]
        [HttpGet]
        public IEnumerable<Photo> GetPhotosOfThePlace(long idPlace)
        {
             return repository.GetPlacePhotos(idPlace);
        }
    }
}
