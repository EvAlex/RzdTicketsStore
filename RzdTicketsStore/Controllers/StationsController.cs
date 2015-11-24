using RzdTicketsStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RzdTicketsStore.Controllers
{
    public class StationsController : Controller
    {
        // GET: Stations
        public ActionResult Index()
        {
            var db = new RzdTicketsDb();
            var stations = db.GetStations();
            return View(stations);
        }
    }
}