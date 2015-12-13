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

        //  GET: Trips/5
        [Route("Trips/{tripId}")]
        public ActionResult Details(int tripId)
        {
            var db = new RzdTicketsDb();
            var trip = db.GetTrip(tripId);
            var tickets = db.GetTickets(trip);
            var model = new TripDetailsViewModel(trip, tickets);
            return View(model);
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

        // GET: Trips/Edit
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var db = new RzdTicketsDb();
            var trip = db.GetTrip(id);
            return View(trip);
        }

        // POST: Trips/Edit
        [HttpPost]
        public ActionResult Edit(TrainTrip trip)
        {
            var db = new RzdTicketsDb();
            db.UpdateTrip(trip);
            return RedirectToAction("Index");
        }

        // POST: Trips/Delete
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var db = new RzdTicketsDb();
            db.DeleteTrip(id);
            return RedirectToAction("Index");
        }
    }
}