﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Models
{
    public class User_Organization
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public Guid OrganizationRightId { get; set; }
        public OrganizationRight UserOrganizationRight { get; set; }
    }
}
