namespace GabsHybridApp.Shared.Models;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public static List<Team> SeedData()
    {
        return new List<Team>
        {
            new() { Id = 1, Name = "Team 1 Alpha" },
            new() { Id = 2, Name = "Team 2 Bravo" },
            new() { Id = 3, Name = "Team 3 Charlie" },
            new() { Id = 4, Name = "Team 4 Delta" }
        };
    }
}
