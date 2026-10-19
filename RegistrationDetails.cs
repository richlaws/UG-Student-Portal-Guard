using System.Collections.Generic;

namespace s;

public class RegistrationDetails
{
    public string AcademicYear { get; set; } = "";

    public string Qualification { get; set; } = "";

    public string Semester { get; set; } = "";

    public string Level { get; set; } = "";

    public List<string> Courses { get; set; } = new();
}
