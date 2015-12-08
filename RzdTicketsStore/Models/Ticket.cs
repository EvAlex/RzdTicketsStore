using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RzdTicketsStore.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        public TrainTrip Trip { get; set; }

        public double Cost { get; set; }

        public DateTime? BookingTime { get; set; }

        public Customer Customer { get; set; }

        public int WagonNumber { get; set; }

        public int SeatNumber { get; set; }
    }
}