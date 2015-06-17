﻿using Nearsens.DataAccess;
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
    }
}
