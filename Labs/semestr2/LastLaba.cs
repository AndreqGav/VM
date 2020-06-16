using System;
using System.Collections.Generic;
using System.IO;
using VichMat.Solution;

namespace WelcomeToVichMat.Labs.semestr2
{
    // TODO Проверка устойчивости.
    public class LastLaba : BaseLaba
    {
        private class ItemSource
        {
            public int Number { get; set; }
            public double X { get; set; }
            public double T { get; set; }
            public double Yij { get; set; }
            public double Rij { get; set; }
        }

        private double[] _x;
        private double[] _t;

        private double _mu1(double x) => x;
        private double _mu2(double t) => _a + t;
        private double _mu3(double t) => _b + t;
        private double _f(double x, double t) => 1;
        private double _u(double x, double t) => x + t;

        private double _a, _b, _T, _k, _sigma;

        private int _N, _M;
        private double _h, _teta;

        private double[] _y0;
        private double[] _y;

        private readonly List<ItemSource> _items = new List<ItemSource>();

        public LastLaba()
        {
            accuracyOutput = null;
        }

        public override void ReadData(StreamReader reader)
        {
            _a = 2;
            _b = 4;
            _T = 0.05;
            _sigma = 5;

            _k = 1;

            _h = 0.5;
            _teta = 0.01;

            _N = (int)Math.Abs((_b - _a) / _h);
            _M = (int)Math.Abs(_T / _teta);
            _x = new double[_N + 1];
            _t = new double[_M + 1];

            _y0 = new double[_N + 1];
            _y = new double[_N + 1];
        }

        public override (Error error, string data) CheckData()
        {
            return (Error.None, null);
        }

        public override void WriteData(StreamWriter writer)
        {
            solution.ShowTable(_items);
        }

        public override void Solve()
        {
            for (var i = 0; i < _N + 1; i++)
            {
                _x[i] = _a + i * _h;
            }

            for (var j = 0; j < _M + 1; j++)
            {
                _t[j] = j * _teta;
            }

            ApplyInitialConditions();

            for (var j = 0; j < _M; j++)
            {
                CalculateNextLayer(j);
            }
        }

        private void SaveLayer(int j)
        {
            for (var i = 0; i < _N + 1; i++)
            {
                _items.Add(new ItemSource
                {
                    Number = j,
                    X = _x[i],
                    T = _t[j],
                    Yij = _y[i],
                    Rij = Math.Abs(_y[i] - _u(_x[i], _t[j]))
                });
            }
        }

        private void ApplyInitialConditions()
        {
            for (var i = 0; i < _N + 1; i++)
            {
                _y[i] = _mu1(_x[i]);
            }
            SaveLayer(0);
            _y0 = _y;
        }

        private void CalculateNextLayer(int j)
        {
            // Явная схема.
            if (Math.Abs(_sigma) < double.Epsilon)
            {
                _y[0] = _mu2(_t[j + 1]);
                _y[_N] = _mu3(_t[j + 1]);

                for (var i = 1; i < _N; i++)
                {
                    _y[i] = _y0[i] + _teta * _k / (_h * _h) * (_y0[i - 1] - 2 * _y0[i] + _y0[i + 1]) + _teta * _f(_x[i], _t[j]);
                }
                SaveLayer(j + 1);

                _y0 = _y;
            }
            else // Неявная схема.
            {
                var vectors = CreateVectors();
                var a = vectors.a;
                var b = vectors.b;
                var c = vectors.c;

                var f = new double[_N + 1];
                f[0] = _mu2(_t[j + 1]);
                f[_N] = _mu3(_t[j + 1]);

                for (var i = 1; i < _N; i++)
                {
                    f[i] = -_h * _h / (_k * _teta * _sigma) * _y0[i] - (1 - _sigma) / _sigma * (_y0[i - 1] - 2 * _y0[i] + _y0[i + 1]) - _h * _h / (_k * _sigma) * _f(_x[i], _t[j]);
                }
                SolveMtp(_N + 1, a, b, c, f, _y);
                SaveLayer(j + 1);

                _y0 = _y;
            }

        }

        private (double[] a, double[] b, double[] c) CreateVectors()
        {
            var a = new double[_N + 1];
            var b = new double[_N + 1];
            var c = new double[_N + 1];

            for (var i = 0; i < _N + 1; i++)
            {
                a[i] = i == 0 || i == _N ? 0 : 1;
                b[i] = i == 0 || i == _N ? 0 : 1;
                c[i] = i == 0 || i == _N ? 1 : -(2 + _h * _h / (_k * _teta * _sigma));
            }

            return (a, b, c);
        }

        void SolveMtp(int n, double[] a, double[] b, double[] c, double[] f, double[] x)
        {
            for (var i = 1; i < n; i++)
            {
                var m = a[i] / c[i - 1];
                c[i] = c[i] - m * b[i - 1];
                f[i] = f[i] - m * f[i - 1];
            }

            x[n - 1] = f[n - 1] / c[n - 1];

            for (var i = n - 2; i >= 0; i--)
            {
                x[i] = (f[i] - b[i] * x[i + 1]) / c[i];
            }
        }
    }
}
