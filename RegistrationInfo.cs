using System.Collections.Generic;

namespace s;

public class RegistrationInfo
{
    // Student Details
    public string StudentName { get; set; } = "";

    public string QualificationCode { get; set; } = "";

    public string QualificationName { get; set; } = "";

    // Academic Session
    public string AcademicYear { get; set; } = "";

    public string Semester { get; set; } = "";

    public string Level { get; set; } = "";

    // Registration Period
    public string StartDate { get; set; } = "";

    public string EndDate { get; set; } = "";

    // Registered Courses
    public List<RegisteredCourse> RegisteredCourses { get; set; } = new();
}
