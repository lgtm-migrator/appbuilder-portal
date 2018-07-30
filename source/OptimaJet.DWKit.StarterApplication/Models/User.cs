﻿using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Models;

namespace Optimajet.DWKit.StarterApplication.Models
{
    [Table("User")]
    public class User : Identifiable
    {
        [Attr("name")]
        public string Name { get; set; }

        [Attr("given-name")]
        public string GivenName { get; set; }

        [Attr("family-name")]
        public string FamilyName { get; set; }

        [Attr("email")]
        public string Email { get; set; }

        [Attr("phone")]
        public string Phone { get; set; }

        [Attr("timezone")]
        public string Timezone { get; set; }

        [Attr("locale")]
        public string Locale { get; set; }

        [Attr("is-locked")]
        public bool IsLocked { get; set; }

        [Attr("auth0Id")]
        public string ExternalId { get; set; }

        //[HasMany("ownedOrganizations")]
        //public virtual List<Organization> OwnedOrganizations { get; set; }

        [HasMany("organization-memberships", Link.None, canInclude: true)]
        public virtual List<OrganizationMembership> OrganizationMemberships { get; set; }

        [NotMapped]
        public IEnumerable<int> OrganizationIds => OrganizationMemberships.Select(o => o.OrganizationId);
    }
}
