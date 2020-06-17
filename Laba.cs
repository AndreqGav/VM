using System;
using System.IO;
using VichMat.Solution;

namespace WelcomeToVichMat
{
    public interface ILaba
    {
        void ReadData(StreamReader reader);
        (Error error, string data) CheckData();
        void WriteData(StreamWriter writer);
        void Solve();
    }


    public abstract class BaseLaba : ILaba
    {
        public readonly Solution solution;
        protected int? accuracyOutput = 5;

        protected BaseLaba()
        {
            this.solution = new Solution(this);
        }

        protected double Round(double a) =>
            accuracyOutput != null ? Math.Round(a, accuracyOutput.Value) : a;

        public abstract void ReadData(StreamReader reader);
        public abstract (Error error, string data) CheckData();
        public abstract void WriteData(StreamWriter writer);
        public abstract void Solve();
    }
}
