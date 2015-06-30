using Nearsens.DataAccess;
using Nearsens.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.IO;

namespace Nearsens.Web.Controllers
{
    public class PlacesController : ApiController
    {
        SqlPlacesRepository repository = new SqlPlacesRepository();

        // GET: api/Places
        //public IEnumerable<GetNearestPlacesQuery> GetNearestPlaces(double lat, double lng)
        //{
        //    return repository.GetNearestPlaces(lat, lng);
        //}

        public IEnumerable<GetNearestPlacesQuery> GetNearestPlacesWithFilters(double lat, double lng, int? distanceLimit = null, string category = null, string subcategory = null)
        {
            return repository.GetNearestPlacesWithFilters(lat, lng, distanceLimit, category, subcategory);
        }

        // GET: api/Places/5
        public GetPlaceQuery Get(long id)
        {
            return repository.GetPlaceById(id);
        }

        [Authorize]
        public IEnumerable<GetPlacesByUserIdQuery> GetPlacesByUserId()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return repository.GetPlacesByUserId(userId);
        }

        // POST: api/Places
        [Authorize]
        public long Post([FromBody]Place place)
        {
            if (!ModelState.IsValid) return -1;

            place.UserId = HttpContext.Current.User.Identity.GetUserId();
            return repository.InsertPlace(place);
        }

        // PUT: api/Places/5
        [Authorize]
        public void Put(Place place)
        {
            repository.UpdatePlace(place);
        }

        // DELETE: api/Places/5
        [Authorize]
        public void Delete(long id)
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            string pathToDelete = HttpContext.Current.Server.MapPath("~/Images/" + userId + "/" + id);
            repository.DeletePlace(id);
            if (Directory.Exists(pathToDelete))
                Directory.Delete(pathToDelete, true);
        }
    }
}
