using Nearsens.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nearsens.DataAccess;
using Microsoft.AspNet.Identity;

namespace Nearsens.Web.Controllers
{
    public class HomeController : Controller
    {

        private ModelList list = new ModelList();
        private SqlPlacesRepository placesRepository = new SqlPlacesRepository();
        private SqlOffersRepository offersRepository = new SqlOffersRepository();
        private SqlSubcategoriesRepository subcategoriesRepository = new SqlSubcategoriesRepository();


        [Authorize]
        public ActionResult Index()
        {
            var userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            list = new ModelList
            {
                places = placesRepository.GetPlacesByUserId(userId)
                ,
                subcategories = subcategoriesRepository.GetAll()
            };
            return View(list);
        }

        [Authorize]
        public ActionResult PlaceDetail(long id)
        {
            string userId = placesRepository.GetIdUserOfThePlace(id);
            string userLogin = System.Web.HttpContext.Current.User.Identity.GetUserId();
            if (userId.Equals(userLogin))
            {
                list = new ModelList
                {
                    subcategories = subcategoriesRepository.GetAll(),
                    place = placesRepository.GetPlaceById(id)

                };
                return View(list);
            }
            else
            {
                return View("../Shared/Error");
            }
        }

        [Authorize]
        public ActionResult PlaceOffers(long id)
        {
            list = new ModelList
            {
                offersByPlace = offersRepository.GetOffersByPlaceId(id)

            };
            return View(list);
        }

        [Authorize]
        public ActionResult OfferDetail(long id)
        {
            list = new ModelList
            {
                offerDetail = offersRepository.GetOfferById(id)

            };
            return View(list);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}