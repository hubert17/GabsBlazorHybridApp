using System.ComponentModel.DataAnnotations.Schema;

namespace GabsHybridApp.Shared.Models;

public class ResponseDriver
{
    public int Id { get; set; }
    public string Name { get; set; }


    [ForeignKey(nameof(Response))]
    public int ResponseId { get; set; }
    public Response Response { get; set; }
}
