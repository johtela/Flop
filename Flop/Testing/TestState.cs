namespace Flop.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Collections;

    public enum TestPhase { Generate, StartShrink, Shrink }

    public class TestState
    {
        public readonly TestPhase Phase;
        public readonly Random Random;
        public int Size;
        public string Label;
        public int SuccessfulTests;
        public int DiscardedTests;
        public Map<string, int> Classes = Map<string, int>.Empty;
        public int CurrentValue;
        public readonly List<object> Values;
        public readonly List<List<object>> ShrunkValues;

        public TestState(TestPhase phase, int seed, int size) :
            this (phase, seed, size, null, null)
        {}

        public TestState(TestPhase phase, int seed, int size, List<object> values, 
            List<List<object>> shrunkValues)
        {
            Phase = phase;
            Random = new Random(seed);
            Size = size;
            Values = values ?? new List<object>();
            ShrunkValues = shrunkValues;
        }

        public void ResetValues ()
        {
            if (Phase == TestPhase.Generate)
            {
                CurrentValue = 0;
                Values.Clear();
            }
        }

    }
}
