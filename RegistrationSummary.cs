using System.Collections.Generic;

namespace s;

public class RegistrationSummary
{
    public string StudentId { get; set; } = "";

    public string Department { get; set; } = "";

    public string Level { get; set; } = "";

    public string Semester { get; set; } = "";

    public int TotalCredits { get; set; }

    public List<string> SelectedCourses { get; set; } = new();
}
