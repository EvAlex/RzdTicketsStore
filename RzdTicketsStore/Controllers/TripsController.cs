using RzdTicketsStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RzdTicketsStore.Controllers
{
    public class TripsController : Controller
    {
        // GET: Trips
        public ActionResult Index()
        {
            var db = new RzdTicketsDb();
            var trips = db.GetTrips();
            return View(trips);
        }

        // GET: Trips/Create
        [HttpGet]
        public ActionResult Create()
        {
            var db = new RzdTicketsDb();
            var station = db.GetStations();
            return View(station);
        }

        // POST: Trips/Create
        [HttpPost]
        public ActionResult Create(TrainTrip trip)
        {
            var db = new RzdTicketsDb();
            db.InsertTrip(trip);
            return RedirectToAction("Index");
        }
    }
}