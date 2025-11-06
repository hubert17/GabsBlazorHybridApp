namespace GabsHybridApp.Shared.Models;

public class NatureIncidentIllness
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int ClassificationCode { get; set; }

    public static List<NatureIncidentIllness> SeedData()
    {
        return new List<NatureIncidentIllness>
        {
            // Code 1
            new() { Id = 1,  Name = "Medical Emergency",        ClassificationCode = 1 },
            new() { Id = 2,  Name = "Medical Non-emergency",    ClassificationCode = 1 },
            new() { Id = 3,  Name = "OB",                       ClassificationCode = 1 },
            new() { Id = 4,  Name = "Dialysis",                 ClassificationCode = 1 },

            // Code 2
            new() { Id = 5,  Name = "Vehicular Accident",       ClassificationCode = 2 },
            new() { Id = 6,  Name = "Shooting",                 ClassificationCode = 2 },
            new() { Id = 7,  Name = "Stabbing/Hacking",         ClassificationCode = 2 },
            new() { Id = 8,  Name = "Mauling",                  ClassificationCode = 2 },
            new() { Id = 9,  Name = "Drowning",                 ClassificationCode = 2 },
            new() { Id = 10, Name = "Fall",                     ClassificationCode = 2 },
            new() { Id = 11, Name = "Others",                   ClassificationCode = 2 },

            // Code 3
            new() { Id = 12, Name = "Fire Incident",            ClassificationCode = 3 },

            // Code 4
            new() { Id = 13, Name = "Flood",                    ClassificationCode = 4 },
            new() { Id = 14, Name = "Landslide",                ClassificationCode = 4 },
        };
    }
}
