using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Linq;

namespace s;

public partial class AdminWindow : Window
{
    private Course selectedCourse;
    
    public AdminWindow()
    {
        InitializeComponent();
        
        OnRequireAllCoreChanged(null, new RoutedEventArgs());
        
        LoadCourseList();
        
        ResetForm(); // Start in Add Mode
    }
    
    private void LoadCourseList(string search = "")
{
    try
    {
        CoursesListBox.Items.Clear();

        using var db = new AppDbContext();
        
        var query = db.Courses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToUpper();

            query = query.Where(c =>
                c.CourseCode.ToUpper().Contains(search) ||
                c.CourseName.ToUpper().Contains(search));
        }

        var courses = query
            .OrderBy(c => c.Level)
            .ThenBy(c => c.Semester)
            .ThenBy(c => c.CourseCode)
            .ToList();

        foreach (var course in courses)
        {
             CoursesListBox.Items.Add(course);
        }
    }
    catch (Exception ex)
    {
        StatusLabel.Text = "" + ex.Message;
    }
}

private string CheckDuplicateCourse(
    string department,
    int level,
    int semester,
    string code,
    string title)
{
    using var db = new AppDbContext();

    code = code.Trim().ToUpper();
    title = title.Trim().ToUpper();

    // Case 1: Exact duplicate
    var exact = db.Courses.FirstOrDefault(c =>
        c.Department == department &&
        c.Level == level &&
        c.Semester == semester &&
        c.CourseCode.ToUpper() == code &&
        c.CourseName.ToUpper() == title);

    if (exact != null)
    {
        return
            $"This course already exists.\n\n" +
            $"{exact.CourseCode} - {exact.CourseName}\n" +
            $"{exact.Department}\n" +
            $"Level {exact.Level}\n" +
            $"Semester {exact.Semester}\n\n" +
            $"Use Edit if you want to modify it.";
    }

    // Case 2: Same code, different title
    var codeExists = db.Courses.FirstOrDefault(c =>
        c.CourseCode.ToUpper() == code);

    if (codeExists != null &&
        codeExists.CourseName.ToUpper() != title)
    {
        return
            $"{code} already exists.\n\n" +
            $"Existing title:\n" +
            $"{codeExists.CourseName}\n\n" +
            $"Please edit the existing course instead.";
    }

    // Case 3: Same title, different code
    var titleExists = db.Courses.FirstOrDefault(c =>
        c.CourseName.ToUpper() == title);

    if (titleExists != null &&
        titleExists.CourseCode.ToUpper() != code)
    {
        return
            $"This course title already exists.\n\n" +
            $"{title}\n\n" +
            $"Existing code: {titleExists.CourseCode}";
    }

    return "";
}

    private void OnAddCourseClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DepartmentBox.SelectedItem == null ||
                LevelBox.SelectedItem == null ||
                SemesterBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(CourseCodeBox.Text) ||
                string.IsNullOrWhiteSpace(CourseNameBox.Text) ||
                string.IsNullOrWhiteSpace(CreditsBox.Text))
            {
                StatusLabel.Text = "Fill all fields";
                return;
            }
            
            string department =
                ((ComboBoxItem)DepartmentBox.SelectedItem).Content.ToString();

            int level =
            int.Parse(((ComboBoxItem)LevelBox.SelectedItem).Content.ToString());

            int semester =
    int.Parse(((ComboBoxItem)SemesterBox.SelectedItem).Content.ToString());

            string code = CourseCodeBox.Text.Trim().ToUpper();

            string title = CourseNameBox.Text.Trim();

            string duplicateMessage = CheckDuplicateCourse(
               department,
               level,
               semester,
               code,
               title);

            if (!string.IsNullOrEmpty(duplicateMessage))
           {
                StatusLabel.Text = duplicateMessage;
                return;
           } 

            var course = new Course
            {
                Department = department,
                Level = level,
                Semester = semester,
                CourseCode = code,
                CourseName = title,
                Credits = int.Parse(CreditsBox.Text),
                IsCore = IsCoreBox.IsChecked == true
            };

            using (var db = new AppDbContext())
            {
                db.Courses.Add(course);
                db.SaveChanges();
                
                StatusLabel.Text = "Course added successfully";
            }

            // Clear fields
            LoadCourseList();
            ResetForm();
            
            StatusLabel.Text = "Course added successfully";
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            StatusLabel.Text = "Course already exists in database";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Error: " + ex.Message;
        }
    }
    
    private void OnRefreshClick(object? sender, RoutedEventArgs e)
{
    LoadCourseList();
    ResetForm();
}

private void OnSearchChanged(object? sender, TextChangedEventArgs e)
{
    LoadCourseList(SearchBox.Text);
}

private void ResetForm()
{
    selectedCourse = null;

    CoursesListBox.SelectedItem = null;

    DepartmentBox.SelectedItem = null;
    LevelBox.SelectedItem = null;
    SemesterBox.SelectedItem = null;

    CourseCodeBox.Text = "";
    CourseNameBox.Text = "";
    CreditsBox.Text = "";
    IsCoreBox.IsChecked = false;

    CourseCodeBox.IsReadOnly = false;
    CourseCodeBox.Background = null;

    EditButton.IsEnabled = false;
    DeleteButton.IsEnabled = false;

    StatusLabel.Text = "";
}

private void OnEditClick(object sender, RoutedEventArgs e)
{
    try
    {
        if (selectedCourse == null)
        {
            StatusLabel.Text = "No course selected";
            return;
        }

        // Update fields (except CourseCode)
        selectedCourse.Department = ((ComboBoxItem)DepartmentBox.SelectedItem).Content.ToString();
        selectedCourse.Level = int.Parse(((ComboBoxItem)LevelBox.SelectedItem).Content.ToString());
        selectedCourse.Semester = int.Parse(((ComboBoxItem)SemesterBox.SelectedItem).Content.ToString());

        selectedCourse.CourseName = CourseNameBox.Text;
        selectedCourse.Credits = int.Parse(CreditsBox.Text);
        selectedCourse.IsCore = IsCoreBox.IsChecked == true;

        using (var db = new AppDbContext())
        {
            db.Courses.Update(selectedCourse);
            db.SaveChanges();
        }

        StatusLabel.Text = "Course updated successfully";

        LoadCourseList();
    }
    catch (Exception ex)
    {
        StatusLabel.Text = "Error: " + ex.Message;
    }
}

private void OnSaveRuleClick(object? sender, RoutedEventArgs e)
{
    try
    {
        if (RuleDepartmentBox.SelectedItem == null ||
            RuleLevelBox.SelectedItem == null ||
            RuleSemesterBox.SelectedItem == null ||
            string.IsNullOrWhiteSpace(RequiredElectivesBox.Text) ||
            string.IsNullOrWhiteSpace(MinCreditsBox.Text) ||
            string.IsNullOrWhiteSpace(MaxCreditsBox.Text))
        {
            StatusLabel.Text = "Fill all registration rule fields.";
            return;
        }

        if (RequireAllCoreBox.IsChecked != true &&
            string.IsNullOrWhiteSpace(RequiredCoreBox.Text))
        {
            StatusLabel.Text = "Enter the required number of core courses.";
            return;
        }

        using var db = new AppDbContext();

        string department =
            ((ComboBoxItem)RuleDepartmentBox.SelectedItem).Content.ToString();

        int level =
    int.Parse(((ComboBoxItem)RuleLevelBox.SelectedItem).Content.ToString());

        int semester =
    int.Parse(((ComboBoxItem)RuleSemesterBox.SelectedItem).Content.ToString());

        bool requireAllCore =
    RequireAllCoreBox.IsChecked == true;

        int requiredCore =
    requireAllCore ? 0 : int.Parse(RequiredCoreBox.Text);

        int requiredElectives =
    int.Parse(RequiredElectivesBox.Text);

        int minCredits =
    int.Parse(MinCreditsBox.Text);

        int maxCredits =
    int.Parse(MaxCreditsBox.Text);

        // Look for an existing rule
        var rule = db.RegistrationRules.FirstOrDefault(r =>
            r.Department == department &&
            r.Level == level &&
            r.Semester == semester);

        if (rule == null)
        {
            // Create a new rule
            rule = new RegistrationRule();

            db.RegistrationRules.Add(rule);
        }

        // Fill the rule (works for both insert and update)
       rule.Department = department;
       rule.Level = level;
       rule.Semester = semester;
       rule.RequireAllCore = requireAllCore;
       rule.RequiredCore = requiredCore;
       rule.RequiredElectives = requiredElectives;
       rule.MinCredits = minCredits;
       rule.MaxCredits = maxCredits;

       db.SaveChanges();
       
       LoadRegistrationRule();

       StatusLabel.Text = "Registration rule saved successfully.";
    }
    catch (Exception ex)
    {
        StatusLabel.Text = ex.Message;
    }
}

private void OnDeleteClick(object? sender, RoutedEventArgs e)
{
    try
    {
        if (selectedCourse == null)
        {
            StatusLabel.Text = "Please select a course first.";
            return;
        }

        using var db = new AppDbContext();

        var course = db.Courses.Find(selectedCourse.Id);

        if (course == null)
        {
            StatusLabel.Text = "Course not found.";
            return;
        }

        db.Courses.Remove(course);
        db.SaveChanges();

        StatusLabel.Text = $"{course.CourseCode} deleted.";

        LoadCourseList();
        ResetForm();
        
        StatusLabel.Text = $"{course.CourseCode} deleted.";
    }
    catch (Exception ex)
    {
        StatusLabel.Text = "" + ex.Message;
    }
}

private void OnRequireAllCoreChanged(object? sender, RoutedEventArgs e)
{
    bool requireAll = RequireAllCoreBox.IsChecked == true;

    RequiredCoreBox.IsEnabled = !requireAll;

    if (requireAll)
    {
        RequiredCoreBox.Text = "";
    }
}

private void OnRuleSelectionChanged(object? sender, SelectionChangedEventArgs e)
{
    LoadRegistrationRule();
}

private void LoadRegistrationRule()
{
    try
    {
        if (RuleDepartmentBox.SelectedItem == null ||
            RuleLevelBox.SelectedItem == null ||
            RuleSemesterBox.SelectedItem == null)
        {
            return;
        }

        string department =
            ((ComboBoxItem)RuleDepartmentBox.SelectedItem).Content.ToString();

        int level =
            int.Parse(((ComboBoxItem)RuleLevelBox.SelectedItem).Content.ToString());

        int semester =
            int.Parse(((ComboBoxItem)RuleSemesterBox.SelectedItem).Content.ToString());

        using var db = new AppDbContext();

        var rule = db.RegistrationRules.FirstOrDefault(r =>
            r.Department == department &&
            r.Level == level &&
            r.Semester == semester);

        if (rule == null)
        {
            RequireAllCoreBox.IsChecked = true;
            RequiredCoreBox.Text = "";
            RequiredElectivesBox.Text = "";
            MinCreditsBox.Text = "";
            MaxCreditsBox.Text = "";

            StatusLabel.Text = "No existing rule found.";
            return;
        }

        RequireAllCoreBox.IsChecked = rule.RequireAllCore;

        if (rule.RequireAllCore)
            RequiredCoreBox.Text = "";
        else
            RequiredCoreBox.Text = rule.RequiredCore.ToString();

        RequiredElectivesBox.Text = rule.RequiredElectives.ToString();
        MinCreditsBox.Text = rule.MinCredits.ToString();
        MaxCreditsBox.Text = rule.MaxCredits.ToString();

        // Keep the RequiredCore textbox enabled/disabled correctly
        OnRequireAllCoreChanged(null, new RoutedEventArgs());

        StatusLabel.Text = "Existing rule loaded.";
    }
    catch (Exception ex)
    {
        StatusLabel.Text = "" + ex.Message;
    }
}

private void OnCourseSelected(object sender, SelectionChangedEventArgs e)
{
    if (CoursesListBox.SelectedItem == null)
        return;

    selectedCourse = CoursesListBox.SelectedItem as Course;

    if (selectedCourse == null)
        return;

    // Fill form fields
    DepartmentBox.SelectedItem = DepartmentBox.Items
        .Cast<ComboBoxItem>()
        .FirstOrDefault(i => i.Content.ToString() == selectedCourse.Department);

    LevelBox.SelectedItem = LevelBox.Items
        .Cast<ComboBoxItem>()
        .FirstOrDefault(i => i.Content.ToString() == selectedCourse.Level.ToString());

    SemesterBox.SelectedItem = SemesterBox.Items
        .Cast<ComboBoxItem>()
        .FirstOrDefault(i => i.Content.ToString() == selectedCourse.Semester.ToString());

    CourseCodeBox.Text = selectedCourse.CourseCode;
    CourseNameBox.Text = selectedCourse.CourseName;
    CreditsBox.Text = selectedCourse.Credits.ToString();
    IsCoreBox.IsChecked = selectedCourse.IsCore;
    
    DeleteButton.IsEnabled = true;
    EditButton.IsEnabled = true;
    
    CourseCodeBox.IsReadOnly = true;
    CourseCodeBox.Background = Avalonia.Media.Brushes.LightGray;
}
}
