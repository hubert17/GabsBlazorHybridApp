using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GabsHybridApp.Shared.Models
{
    public class Responder
    {
        public int Id { get; set; }
        public string LastName { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string PhoneNo { get; set; } = default!;

        [ForeignKey(nameof(Team))]
        public int? TeamId { get; set; }
        public Team? Team { get; set; }

        public static List<Responder> SeedData()
        {
            return new List<Responder>
            {
                new() { Id = 1,  FirstName = "Coco",     LastName = "Martin",   Address = "Brgy. Poblacion, Quezon, Bukidnon",     PhoneNo = "0917-123-4567", TeamId = 1 },
                new() { Id = 2,  FirstName = "Kathryn",  LastName = "Bernardo", Address = "Brgy. Kiokong, Quezon, Bukidnon",       PhoneNo = "0908-234-5678", TeamId = 2 },
                new() { Id = 3,  FirstName = "Daniel",   LastName = "Padilla",  Address = "Brgy. Butong, Quezon, Bukidnon",        PhoneNo = "0999-345-6789", TeamId = 3 },
                new() { Id = 4,  FirstName = "Liza",     LastName = "Soberano", Address = "Brgy. Salawagan, Quezon, Bukidnon",     PhoneNo = "0927-456-7890", TeamId = 4 },
                new() { Id = 5,  FirstName = "Enrique",  LastName = "Gil",      Address = "Brgy. San Jose, Quezon, Bukidnon",      PhoneNo = "0956-567-8901", TeamId = 1 },
                new() { Id = 6,  FirstName = "Nadine",   LastName = "Lustre",   Address = "Brgy. Lumintao, Quezon, Bukidnon",      PhoneNo = "0910-678-9012", TeamId = 2 },
                new() { Id = 7,  FirstName = "James",    LastName = "Reid",     Address = "Brgy. Libertad, Quezon, Bukidnon",      PhoneNo = "0945-789-0123", TeamId = 3 },
                new() { Id = 8,  FirstName = "Vice",     LastName = "Ganda",    Address = "Brgy. Cawayan, Quezon, Bukidnon",       PhoneNo = "0938-890-1234", TeamId = 4 },
                new() { Id = 9,  FirstName = "Bea",      LastName = "Alonzo",   Address = "Brgy. Kahabaan, Quezon, Bukidnon",      PhoneNo = "0918-901-2345", TeamId = 1 },
                new() { Id = 10, FirstName = "Piolo",    LastName = "Pascual",  Address = "Brgy. Magsaysay, Quezon, Bukidnon",     PhoneNo = "0920-012-3456", TeamId = 2 },
            };
        }
    }
}
