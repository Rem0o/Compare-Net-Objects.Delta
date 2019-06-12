using KellermanSoftware.CompareNetObjects;
using System.Collections.Generic;
using Xunit;

namespace CompareNetObjects.Delta.Tests
{
    public class ExpectedToActualActionsTests
    {
        [Fact]
        public void Expected_ExpectedToActual_EqualsToActual()
        {
            Company expected = new Company()
            {
                Name = "Retard Inc.",
                Employees = new List<Employee>
                {
                    new Employee
                    {
                        AnnualSalary = 50_000,
                        Name = "Johnny Go",
                        Position = "Boss"
                    },
                    new Employee
                    {
                        AnnualSalary = 35_000,
                        Name = "Will Do",
                        Position = "Suck-Up"
                    }
                }
            };

            Company actual = new Company()
            {
                Name = "Idiot Inc.",
                Employees = new List<Employee>
                {
                    new Employee
                    {
                        AnnualSalary = 75_000,
                        Name = "Johnny Stop",
                        Position = "Boss"
                    }
                }
            };

            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.MaxDifferences = int.MaxValue;

            ComparisonResult<Company> comparisonResult = compareLogic.Compare<Company>(expected, actual);

            comparisonResult.ApplyExpectedToActualDelta(expected);

            // expected should now be the same as actual
            ComparisonResult<Company> newComparisonResult = compareLogic.Compare<Company>(expected, actual);
            Assert.True(newComparisonResult.AreEqual);

            comparisonResult.ActualToExpected(expected);

            // expected should be back to its original state
            compareLogic.Compare<Company>(expected, actual)
                .ShouldCompare(comparisonResult);
        }
    }
}
