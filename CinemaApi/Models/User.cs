﻿using System.Collections.Generic;

namespace CinemaApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Roles { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
