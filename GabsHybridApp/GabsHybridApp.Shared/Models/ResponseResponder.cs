using System.ComponentModel.DataAnnotations.Schema;

namespace GabsHybridApp.Shared.Models;

public class ResponseResponder
{
    public int Id { get; set; }

    [ForeignKey(nameof(Responder))]
    public int ResponserId { get; set; }
    public Responder Responder { get; set; }


    [ForeignKey(nameof(Response))]
    public int ResponseId { get; set; }
    public Response Response { get; set; }

}
