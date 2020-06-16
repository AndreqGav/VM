using System;
using System.Collections.Generic;
using System.IO;
using VichMat.Solution;

namespace WelcomeToVichMat.Labs.semestr2
{
    public class Laba : BaseLaba
    {
        private class ItemSource
        {
            public int Number { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double DeltaU { get; set; }
            public double Dy { get; set; }
            public double DeltaDu { get; set; }
        }

        private double _alpha;
        private double _a;
        private double _b;

        private int _n;
        private double _h;

        private double A;
        private double B;

        private double _eps;

        double[] _arrU;
        double[] _arrW;

        double _start;
        int _maxIteration;
        int _iteration;

        public Laba()
        {
            accuracyOutput = 5;
        }

        public override void ReadData(StreamReader reader)
        {
            var str = reader.ReadLine()?.Split("              ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) ?? new string[] { };
            _a = double.Parse(str[0]);
            _b = double.Parse(str[1]);

            str = reader.ReadLine()?.Split("              ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) ?? new string[] { };
            _n = int.Parse(str[0]);
            _eps = double.Parse(str[1]);
            _maxIteration = int.Parse(str[2]);

            str = reader.ReadLine()?.Split("              ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) ?? new string[] { };
            _alpha = double.Parse(str[0]);

            str = reader.ReadLine()?.Split("              ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) ?? new string[] { };
            A = double.Parse(str[0]);
            B = double.Parse(str[1]);

            _h = (_b - _a) / _n;
            _n++;
            _arrU = new double[_n];
            _arrW = new double[_n];

            _arrU[0] = A;
            _arrW[0] = _alpha;

            _start = _a;
        }

        public override (Error error, string data) CheckData()
        {
            if (_b <= _a)
            {
                return (Error.Custom, $"Неверно зада отрезок: [{_a},{_b}]");
            }

            if (_n <= 0)
            {
                return (Error.Custom, $"Кол-во точек должно быть положительным");
            }

            if (_eps <= 0)
            {
                return (Error.Custom, $"Эпсилон должен быть положительным");
            }

            if (_maxIteration <= 0)
            {
                return (Error.Custom, $"Кол-во максимальных итераций должно быть положительным");
            }

            return (Error.None, null);
        }

        public override void WriteData(StreamWriter writer)
        {
            _alpha = _arrW[0];

            var er = _iteration == _maxIteration ? 1 : 0;

            var str = $"IER = {er} L = {_iteration} a = {_alpha,-10:f10} \n";
            Console.WriteLine(str);
            writer.WriteLine(str);
            solution.ShowData(str);
            var items = new List<ItemSource>();

            for (int i = 0; i < _n; i++)
            {
                var x = _start + _h * i;
                var y0 = U(x, 0, 0);
                var dy0 = Du(x, 0, 0);

                var source = new ItemSource
                {
                    Number = i,
                    X = Round(x),
                    Y = Round(y0),
                    DeltaU = Round(Math.Abs(y0 - _arrU[i])),
                    Dy = Round(dy0),
                    DeltaDu = Round(Math.Abs(dy0 - _arrW[i]))
                };
                items.Add(source);

                str = $"№ = {source.Number,-2} X = {source.X,-10:f10} Y = {source.Y,-10:f10} delta U = {source.DeltaU,-10:f10} Y' = {source.Dy,-10:f10} delta U' = {source.DeltaDu,-10:f10}";
                Console.WriteLine(str);
                writer.WriteLine(str);
            }
            solution.ShowTable(items);
        }

        public override void Solve()
        {
            _iteration = 0;

            double tempU = U_b();

            while (Math.Abs(tempU - B) > _eps && _maxIteration > _iteration)
            {
                _arrW[0] -= (tempU - B) / Dphi();
                tempU = U_b();
                _iteration++;
            }

            if (_iteration == _maxIteration)
            {
                solution.SetError(Error.MaxIteration, _maxIteration.ToString(), true);
            }
        }

        // решение (U) = y
        private static double U(double x, double u, double w)
        {
            return x * x; // тест 1, тест 2, тест 3
        }

        // первая производная (U') = w
        private static double Du(double x, double u, double w)
        {
            return 2.0 * x; // тест 1 тест 2 тест 3
        }

        // правая часть (U'') = f
        private static double F(double x = 0, double u = 0, double w = 0)
        {
            //return 2; // тест 1
            //return 2.0 + x * x - u; // тест 2
            return 2.0 + u * w - 2.0 * x * x * x; // тест 3
        }

        // dfdu/duda
        private static double Dfdu(double x, double u, double w)
        {
            //return 0; // тест 1
            //return -1; // тест 2
            return w; // тест 3
        }

        // dfdw/dwda
        private static double Dfdw(double x, double u, double w)
        {
            //return 0; // тест 1 тест 2
            return u; // тест 3
        }

        private double U_b()
        {
            for (int i = 0; i < _n - 1; i++)
            {
                var k1 = _arrW[i];
                var l1 = F(_start + _h * i, _arrU[i], _arrW[i]);
                var k2 = _arrW[i] + _h * l1;
                var l2 = F(_start + _h * (i + 1), _arrU[i] + _h * k1, _arrW[i] + _h * l1);
                _arrU[i + 1] = _arrU[i] + 0.5 * _h * (k1 + k2);
                _arrW[i + 1] = _arrW[i] + 0.5 * _h * (l1 + l2);
            }
            return _arrU[_n - 1];
        }

        private double Dphi()
        {
            double dU = 0;
            double dW = 1;
            for (int i = 0; i < _n - 1; i++)
            {
                var k1 = dW;
                var l1 = Dfdu(_start + _h * i, _arrU[i], _arrW[i]) * dU + Dfdw(_start + _h * i, _arrU[i], _arrW[i]) * dW;
                var k2 = dW + _h * l1;
                var l2 = Dfdu(_start + _h * (i + 1), _arrU[i], _arrW[i]) * (dU + _h * k1) + Dfdw(_start + _h * (i + 1), _arrU[i], _arrW[i]) * (dW + _h * l1);
                dU += 0.5 * _h * (k1 + k2);
                dW += 0.5 * _h * (l1 + l2);
            }

            return dU;
        }
    }
}
