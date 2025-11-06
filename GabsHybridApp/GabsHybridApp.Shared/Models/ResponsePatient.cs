using System.ComponentModel.DataAnnotations.Schema;

namespace GabsHybridApp.Shared.Models;

public class ResponsePatient
{
    public int Id { get; set; }
    public string Name { get; set; } // original: "Name"
    public string Address { get; set; } // original: "Address"
    public int Age { get; set; } // original: "Age"
    public string Gender { get; set; }


    [ForeignKey(nameof(Response))]
    public int ResponseId { get; set; }
    public Response Response { get; set; }

}
