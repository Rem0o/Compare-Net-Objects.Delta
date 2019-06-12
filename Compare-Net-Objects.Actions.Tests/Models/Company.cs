using System.Collections.Generic;
using System.Linq;

namespace CompareNetObjects.Actions.Tests
{
    public class Company
    {
        public string Name { get; set; }
        public List<Employee> Employees { get; set; }
        public decimal TotalAnnualRenenues { get; set; }
        public decimal TotalAnnualLaborCost => Employees.Sum(employee => employee.AnnualSalary);
    }
}
