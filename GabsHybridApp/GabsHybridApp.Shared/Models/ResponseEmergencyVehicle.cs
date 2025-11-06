using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabsHybridApp.Shared.Models
{
    public class ResponseEmergencyVehicle
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PlateNumber { get; set; }


        [ForeignKey(nameof(Response))]
        public int ResponseId { get; set; }
        public Response Response { get; set; }
    }
}
