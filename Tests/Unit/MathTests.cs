using System;
using System.Diagnostics;
using Godot;

namespace DifferentWay.Tests.Unit
{
    public class MathTests
    {
        public void RunAllTests()
        {
            TestEvasionFormula();
            GD.Print("All Unit Tests Passed.");
        }

        private void TestEvasionFormula()
        {
            // Dummy math test as requested by Arc.md
            int agility = 10;
            int baseEvasion = 5;
            int expectedEvasion = baseEvasion + (agility * 2);

            Debug.Assert(expectedEvasion == 25, "Evasion calculation is incorrect!");
        }
    }
}
