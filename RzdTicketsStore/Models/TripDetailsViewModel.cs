using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RzdTicketsStore.Models
{
    public class TripDetailsViewModel
    {
        public TripDetailsViewModel(TrainTrip trip, Ticket[] tickets)
        {
            Trip = trip;
            Tickets = tickets;
        }

        public TrainTrip Trip { get; private set; }

        public Ticket[] Tickets { get; private set; }
    }
}