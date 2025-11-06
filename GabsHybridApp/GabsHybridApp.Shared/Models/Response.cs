using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabsHybridApp.Shared.Models
{
    public class Response
    {
        public int Id { get; set; }

        public DateTime ResponseDate { get; set; }
        public string NatureOfRequest { get; set; } // original: "Nature of Request"
        public RequestBy RequestBy { get; set; } // original: "Caller/Walk-in"

        public List<ResponsePatient> Patients { get; set; }


        [ForeignKey(nameof(NatureOfIncident))]
        public int NatureOfIncidentId { get; set; }
        public virtual NatureIncidentIllness NatureOfIncident { get; set; }

        public string IncidentLocationDescription { get; set; }
        public string GeoLocationCoordinate { get; set; }
        public string GeoLocationAddress { get; set; }
        public string Purok { get; set; }
        public string Barangay { get; set; }
        public string MunicipalityCity { get; set; }
        public string Province { get; set; }

        public string ChiefComplaintInjury { get; set; } // original: "Chief Complaint/Injury"
        public int IncidentLocationId { get; set; } // original: "Location of Incident/Patient"
        public DateTime? Enroute { get; set; } // original: "Enroute"
        public DateTime? AtScene { get; set; } // original: "At Scene"
        public DateTime? FromScene { get; set; } // original: "From Scene"
        public DateTime? AtDestination { get; set; } // original: "At Destination"
        public DateTime? FromDestination { get; set; } // original: "From Destination"
        public DateTime? InService { get; set; } // original: "In Service"
        public double ResponseTime { get; set; } // original: "Response Time"
        public string HandoverLocationFrom { get; set; } // original: "From"
        public string HandoverLocationTo { get; set; } // original: "To"


        [ForeignKey(nameof(RespondingTeam))]
        public int? RespondingTeamId { get; set; }
        public virtual Team RespondingTeam { get; set; }

        public List<ResponseDriver> Drivers { get; set; } // original: "Driver/s"
        public List<ResponseResponder> Responders { get; set; } // original: "Responder/s"
        public List<ResponseEmergencyVehicle> EmergencyVehicles { get; set; }

        // Audit Fields
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

        public bool IsLocked { get; set; } = false;

    }
}
