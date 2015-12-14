using RzdTicketsStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace RzdTicketsStore.Controllers
{
    public class TicketsController : Controller
    {
        // GET: Tickets
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Book(int ticketId)
        {
            var db = new RzdTicketsDb();
            string userId = User.Identity.GetUserId();
            var customer = db.GetCustomer(userId);
            db.BookTicket(ticketId, customer.Id);
            return View();
        }

        public ActionResult Buy(int ticketId)
        {
            var db = new RzdTicketsDb();
            string userId = User.Identity.GetUserId();
            var customer = db.GetCustomer(userId);
            db.BuyTicket(ticketId, customer.Id);
            return View();
        }

        public ActionResult CancelBooking(int ticketId)
        {
            var db = new RzdTicketsDb();
            db.CancelBooking(ticketId);
            return View();
        }

        public ActionResult ReturnTicket(int ticketId)
        {
            var db = new RzdTicketsDb();
            db.ReturnSoldTicket(ticketId);
            return View();
        }
    }
}