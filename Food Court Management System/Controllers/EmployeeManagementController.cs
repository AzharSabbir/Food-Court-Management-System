using System.Collections.Generic;
using System.Web.Mvc;
using Food_Court_Management_System.Models;

namespace Food_Court_Management_System.Controllers
{
    public class EmployeeManagementController : Controller
    {
        // Static data for demonstration
        public static List<EmployeeModel> Employees = new List<EmployeeModel>
        {
            new EmployeeModel { EmployeeID = 101, RestaurantID = 1, Name = "John Doe", Email = "johndoe@example.com", Phone = "123-456-7890", Designation = "Manager" },
            new EmployeeModel { EmployeeID = 102, RestaurantID = 1, Name = "Jane Smith", Email = "janesmith@example.com", Phone = "098-765-4321", Designation = "Chef" },
            new EmployeeModel { EmployeeID = 103, RestaurantID = 1, Name = "Michael Brown", Email = "michaelbrown@example.com", Phone = "112-233-4455", Designation = "Waiter" }
        };

        [HttpGet]
        public ActionResult Index()
        {
            var model = new EmployeeManagementViewModel
            {
                EmployeeList = Employees
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult AddEmployee(EmployeeModel employee)
        {
            employee.EmployeeID = Employees.Count + 101;
            employee.RestaurantID = 1; // Set based on current restaurant session
            Employees.Add(employee);

            return RedirectToAction("Index");
        }
    }
}
