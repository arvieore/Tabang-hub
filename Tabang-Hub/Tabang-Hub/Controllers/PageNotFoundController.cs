using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tabang_Hub.Controllers
{
    public class PageNotFoundController : BaseController
    {
        // GET: PageNotFound
        public ActionResult PageNotFound()
        {
            return View();
        }
    }
}