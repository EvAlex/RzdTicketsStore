using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RzdTicketsStore.Models
{
    public class TrainTrip
    {
        public int Id { get; set; }
        
        public Station DepartureStation { get; set; }

        public DateTime DepartureTime { get; set; }

        public Station ArrivalStation { get; set; }

        public DateTime ArrivalTime { get; set; }
    }
}