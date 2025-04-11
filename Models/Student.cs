namespace seachdemo.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Teacher> Teachers { get; set; } = new List<Teacher>();
    }
    public class Teacher
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
    }

}
