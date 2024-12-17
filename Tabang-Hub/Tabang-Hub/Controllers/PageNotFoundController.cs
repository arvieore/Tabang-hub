using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Controllers
{
    public class PageNotFoundController : BaseController
    {
        // GET: PageNotFound
        public ActionResult PageNotFound()
        {
            var userAccount = _organizationManager.GetUserByUserId(UserId);

            var indexModel = new Lists()
            { 
                userAccount = userAccount,
            };
            return View(indexModel);
        }
    }
}