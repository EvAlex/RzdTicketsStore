using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RzdTicketsStore.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        public Station DepartureStation { get; set; }

        public DateTime DepartureTime { get; set; }

        public Station ArrivalStation { get; set; }

        public DateTime ArrivalTime { get; set; }

        public double Cost { get; set; }

        public DateTime? BookingTime { get; set; }

        public Customer Customer { get; set; }
    }
}