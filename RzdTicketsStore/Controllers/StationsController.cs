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

        // GET: Stations/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View(new Station());
        }

        // POST: Stations/Create
        [HttpPost]
        public ActionResult Create(Station station)
        {
            var db = new RzdTicketsDb();
            db.InsertStation(station);
            return RedirectToAction("Index");
        }

        // GET: Stations/Edit
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var db = new RzdTicketsDb();
            var station = db.GetStation(id);
            return View(station);
        }

        // POST: Stations/Edit
        [HttpPost]
        public ActionResult Edit(Station station)
        {
            var db = new RzdTicketsDb();
            db.UpdateStation(station);
            return RedirectToAction("Index");
        }

        // POST: Stations/Delete
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var db = new RzdTicketsDb();
            db.DeleteStation(id);
            return RedirectToAction("Index");
        }
    }
}