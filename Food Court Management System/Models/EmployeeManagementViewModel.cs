using System.Collections.Generic;

namespace Food_Court_Management_System.Models
{
    public class EmployeeManagementViewModel
    {
        public List<EmployeeModel> EmployeeList { get; set; } = new List<EmployeeModel>();
        public EmployeeModel NewEmployee { get; set; } = new EmployeeModel();
    }
}
