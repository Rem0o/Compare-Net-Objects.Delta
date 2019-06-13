using KellermanSoftware.CompareNetObjects;
using System.Collections.Generic;
using Xunit;

namespace CompareNetObjects.Delta.Tests
{
    public class ExpectedToActualActionsTests
    {
        [Fact]
        public void Expected_ApplyExpectedToActualDelta_EqualsToActual()
        {
            Company expected = GetRetardInc();
            Company actual = GetIdiotInc();
            CompareLogic compareLogic = GetCompareLogic();

            ComparisonResult<Company> comparisonResult = compareLogic.Compare<Company>(expected, actual);

            comparisonResult.ApplyExpectedToActualDelta(expected);

            // expected should now be the same as actual
            ComparisonResult<Company> newComparisonResult = compareLogic.Compare<Company>(expected, actual);
            Assert.True(newComparisonResult.AreEqual, "Both objects should be the same");
        }

        [Fact]
        public void DeltaCreated_NonDeltaPropertiesChanged_NotAffectedByDelta()
        {
            Company expected = GetIdiotInc();
            Company actual = GetRetardInc();
            CompareLogic compareLogic = GetCompareLogic();

            ComparisonResult<Company> comparisonResult = compareLogic.Compare<Company>(expected, actual);

            Delta<Company> delta = comparisonResult.GetExpectedToActualDelta();

            // should not be affected by the delta, even because it was done after the compare.
            expected.TotalAnnualRenenues += 1;
            decimal totalAnnualRevenuesBeforeDelta = expected.TotalAnnualRenenues;

            delta.Apply(expected);

            Assert.Equal(expected.TotalAnnualRenenues, totalAnnualRevenuesBeforeDelta);
        }

        [Fact]
        public void DeltaCreated_ActualChangedAfter_DeltaNotAffected()
        {
            Company expected = GetIdiotInc();
            Company actual = GetRetardInc();
            CompareLogic compareLogic = GetCompareLogic();

            ComparisonResult<Company> comparisonResult = compareLogic.Compare<Company>(expected, actual);

            Delta<Company> delta = comparisonResult.GetExpectedToActualDelta();
            // should not affect the delta function
            int originalCount = actual.Employees.Count;
            actual.Employees.RemoveAt(1);
            
            delta.Apply(expected);

            Assert.Equal(expected.Employees.Count, originalCount);
        }

        private static CompareLogic GetCompareLogic()
        {
            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.MaxDifferences = int.MaxValue;
            return compareLogic;
        }

        private static Company GetIdiotInc()
        {
            return new Company()
            {
                Name = "Idiot Inc.",
                TotalAnnualRenenues = 5_000_000,
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
        }

        private static Company GetRetardInc()
        {
            return new Company()
            {
                Name = "Retard Inc.",
                TotalAnnualRenenues = 5_000_000,
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
        }
    }
}
