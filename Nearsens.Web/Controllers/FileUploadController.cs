using Nearsens.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Nearsens.DataAccess;
using Nearsens.Web.App_Code;

namespace Nearsens.Web.Controllers
{
    public class FileUploadController : ApiController
    {
        // api/FileUpload/

        SqlPlacesRepository placesRepository = new SqlPlacesRepository();
        SqlOffersRepository offersRepository = new SqlOffersRepository();

        [Authorize]
        [HttpPost]
        public async Task<List<string>> UploadPlaceIcon(long placeId)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var userId = HttpContext.Current.User.Identity.GetUserId();
                string relativePath = "Images\\" + userId + "\\" + placeId;
                string uploadPath = HttpContext.Current.Server.MapPath("~\\" + relativePath);

                var messages = await DoSomething(uploadPath);

                placesRepository.InsertIcon(placeId, relativePath + "\\" + messages.First());
                return messages;
            }
            else
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request!");
                throw new HttpResponseException(response);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<List<string>> UploadPlacePhotos(long placeId)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var userId = HttpContext.Current.User.Identity.GetUserId();
                string relativePath = "Images\\" + userId + "\\" + placeId;
                string uploadPath = HttpContext.Current.Server.MapPath("~\\" + relativePath);

                var messages = await DoSomething(uploadPath);

                placesRepository.InsertPlacePhotos(placeId, messages.Select(xx => xx.Insert(0, relativePath + "\\")).ToList());
                return messages;
            }
            else
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request!");
                throw new HttpResponseException(response);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<List<string>> UploadOfferIcon(long offerId, long placeId)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var userId = HttpContext.Current.User.Identity.GetUserId();
                string relativePath = "Images\\" + userId + "\\" + placeId;
                string uploadPath = HttpContext.Current.Server.MapPath("~\\" + relativePath);

                var messages = await DoSomething(uploadPath);

                offersRepository.InsertIcon(offerId, relativePath + "\\" + messages.First());
                return messages;
            }
            else
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request!");
                throw new HttpResponseException(response);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<List<string>> UploadOfferMainPhoto(long offerId, long placeId)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var userId = HttpContext.Current.User.Identity.GetUserId();
                string relativePath = "Images\\" + userId + "\\" + placeId;
                string uploadPath = HttpContext.Current.Server.MapPath("~\\" + relativePath);

                var messages = await DoSomething(uploadPath);

                offersRepository.InsertMainPhoto(offerId, relativePath + "\\" + messages.First());
                return messages;
            }
            else
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request!");
                throw new HttpResponseException(response);
            }
        }
       


        [Authorize]
        [HttpPost]
        public async Task<List<string>> UploadOfferPhotos(long offerId, long placeId)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var userId = HttpContext.Current.User.Identity.GetUserId();
                string relativePath = "Images\\" + userId + "\\" + placeId;
                string uploadPath = HttpContext.Current.Server.MapPath("~\\" + relativePath);

                var messages = await DoSomething(uploadPath);

                offersRepository.InsertOfferPhotos(offerId, messages.Select(xx => xx.Insert(0, relativePath + "\\")).ToList());
                return messages;
            }
            else
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request!");
                throw new HttpResponseException(response);
            }
        }

        private async Task<List<string>> DoSomething(string uploadPath)
        {
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            MyStreamProvider streamProvider = new MyStreamProvider(uploadPath);

            try
            {
                await Request.Content.ReadAsMultipartAsync(streamProvider);
            }
            catch (Exception e)
            {
                ExceptionUtility.LogException(e, "FileUploadController");
            }

            List<string> messages = new List<string>();
            foreach (var file in streamProvider.FileData)
            {
                FileInfo fi = new FileInfo(file.LocalFileName);
                string[] splittedPath = file.LocalFileName.Split('\\');
                string path = splittedPath[splittedPath.Length - 1];
                messages.Add(path);
            }

            return messages;
        }
    }
}
