using System;
using System.Collections.Generic;

namespace EFCoreComputedColumnProblem
{
    public class TaxesPercentage : ValueObject
    {
        public int Value { get; private set; }
        public decimal Multiplier { get; private set; }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public TaxesPercentage(int value)
        {
            Value = value >= 0 && value <= 100 ? value : throw new ArgumentOutOfRangeException($"{nameof(Value)} must be between 0 and 100.");
            Multiplier = 1 + value / 100;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
            yield return Multiplier;
        }

        //■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

        public override string ToString() => $"{Value}";
    }
}
