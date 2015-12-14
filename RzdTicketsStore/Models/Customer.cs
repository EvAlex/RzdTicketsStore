using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RzdTicketsStore.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Surname { get; set; }

        public string Name { get; set; }

        public string Fathersname { get; set; }

        public string PassportNumber { get; set; }

        public DateTime BirthDate { get; set; }
    }
}