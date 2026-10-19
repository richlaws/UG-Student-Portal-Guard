using System;

namespace s;

public class User
{
    public int Id { get; set; }
    public string StudentId { get; set; }
}

public class Course
{
    public int Id { get; set; }

    public string Department { get; set; }

    public int Level { get; set; }
    
    public int Semester { get; set; }

    public string CourseCode { get; set; }

    public string CourseName { get; set; }

    public int Credits { get; set; }

    public bool IsCore { get; set; }
    
    public override string ToString()
    {
         string type = IsCore ? "🟩 CORE" : "🟦 ELECTIVE";

         return $"{type} | L{Level} | S{Semester} | {CourseCode} - {CourseName}";
    }
}

public class RegistrationRule
{
    public int Id { get; set; }

    public string Department { get; set; }

    public int Level { get; set; }

    public int Semester { get; set; }

    public bool RequireAllCore { get; set; }
    
    public int RequiredCore { get; set; }

    public int RequiredElectives { get; set; }

    public int MinCredits { get; set; }

    public int MaxCredits { get; set; }
}

public class RegisteredCourse
{
    public int Id { get; set; }

    public string StudentId { get; set; } = "";

    public string Department { get; set; } = "";

    public int Level { get; set; }

    public int Semester { get; set; }


    // Portal course information

    public string CourseCode { get; set; } = "";

    public string CourseDescription { get; set; } = "";

    public string AcademicBlock { get; set; } = "";

    public string OfferingType { get; set; } = "";

    public string ClassGroup { get; set; } = "";

    public string ExamType { get; set; } = "";


    // Database fields

    public DateTime RegisteredOn { get; set; }

    public string Status { get; set; } = "";
}
