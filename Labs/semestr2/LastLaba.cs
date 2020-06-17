using System;
using System.Collections.Generic;
using System.IO;
using VichMat.Solution;
using WelcomeToVichMat.Extensions;

namespace WelcomeToVichMat.Labs.semestr2
{
    public class LastLaba : BaseLaba
    {
        private class ItemSource
        {
            public int? Слой { get; set; }
            public double? X { get; set; }
            public double? T { get; set; }
            public double? Yij { get; set; }
            public double? Uij { get; set; }
            public double? Rij { get; set; }
            public double? RMax { get; set; }
            public double? RGlobal { get; set; }
        }

        private double[] _x;
        private double[] _t;

        private double _mu1(double x) => x * x; // x
        private double _mu2(double t) => _a * _a + t * t; // _a + t
        private double _mu3(double t) => _b * _b + t * t; // _b + t
        private double _f(double x, double t) => t - _k; // 1
        private double _u(double x, double t) => x * x + t * t; // x + t

        private double _a, _b, _T, _k, _sigma;

        private int _N, _M;
        private double _h, _teta;

        private double[] _y0;
        private double[] _y;

        private readonly List<ItemSource> _items = new List<ItemSource>();
        private int _currentLayer = 0;
        private double _rMax, _rGlobal;
        private bool? _sustainability;

        public LastLaba()
        {
            accuracyOutput = 15;
        }

        public override void ReadData(StreamReader reader)
        {
            var str = reader.ReadLineAndSplit();
            _a = double.Parse(str[0]);
            _b = double.Parse(str[1]);
            _T = double.Parse(str[2]);

            str = reader.ReadLineAndSplit();
            _k = double.Parse(str[0]);
            _sigma = double.Parse(str[1]);


            str = reader.ReadLineAndSplit();
            _h = double.Parse(str[0]);
            _teta = double.Parse(str[1]);

            _N = (int)Math.Abs((_b - _a) / _h);
            _M = (int)Math.Abs(_T / _teta);
            _x = new double[_N + 1];
            _t = new double[_M + 1];

            _y0 = new double[_N + 1];
            _y = new double[_N + 1];
        }

        public override (Error error, string data) CheckData()
        {
            _sustainability = !(Math.Abs(_sigma) < double.Epsilon && _teta > _h * _h / (2 * _k) || _sigma > 0 && _sigma < 0.5);
            var dataShow = _sustainability ?? false ? "УСТОЙЧИВО" : "НЕУСТОЙЧИВО";
            dataShow += "\n" + (Math.Abs(_sigma) < double.Epsilon ? "Явная" : "Неявная");
            solution.ShowData(dataShow);

            if (_b <= _a)
            {
                return (Error.Custom, $"Неверно задан отрезок: [{_a},{_b}]");
            }
            if (_T < 0)
            {
                return (Error.Custom, $"Неверно задано значение параметра T: {_T}");
            }
            if (_sigma < 0)
            {
                return (Error.Custom, $"Параметр сигма должен быть положительным!");
            }
            if (_k < 0)
            {
                return (Error.Custom, $"Параметр c^2 должен быть положительным!");
            }
            if (_h <= 0 || _teta <= 0)
            {
                return (Error.Custom, $"Неверно заданы параметры шага h или teta");
            }
            if (Math.Abs(_mu1(_a) - _mu2(0)) > double.Epsilon || Math.Abs(_mu1(_b) - _mu3(0)) > double.Epsilon)
            {
                return (Error.Custom, $"Убедитесь в правильности краевых и начальных условиях");
            }

            return (Error.None, null);
        }

        public override void WriteData(StreamWriter writer)
        {
            solution.ShowTable(_items);

            writer.WriteLine(_sustainability ?? false ? "УСТОЙЧИВО" : "НЕУСТОЙЧИВО");

            _items.ForEach(itm =>
            {
                if (itm.Слой != null)
                {
                    writer.WriteLine($"Слой№ {itm.Слой}");
                }

                if (itm.Yij.HasValue && itm.X.HasValue && itm.T.HasValue)
                {
                    var i = (int) ((itm.X - _a) / _h);
                    writer.WriteLine($"i = {i, 2} x = {itm.X, -10} Uij = {itm.Yij, -10} U(t, x) = {_u(itm.X ?? 0, itm.T ?? 0), -10} r = {itm.Rij, -5}");
                }

                if (itm.RMax.HasValue)
                {
                    writer.WriteLine($"r_max = {itm.RMax}");
                }

                if (itm.RGlobal.HasValue)
                {
                    writer.WriteLine($"r_global = {itm.RGlobal}");
                }
            });
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

            SaveGlobalR();
        }

        private void SaveLayer(int j)
        {
            _rMax = 0;
            for (var i = 0; i < _N + 1; i++)
            {
                int? layer = null;
                if (j == _currentLayer)
                {
                    layer = j;
                    _currentLayer++;
                }

                var r = Math.Abs(_y[i] - _u(_x[i], _t[j]));
                _rMax = Math.Max(r, _rMax);
                _items.Add(new ItemSource
                {
                    Слой = layer,
                    X = Round(_x[i]),
                    T = Round(_t[j]),
                    Yij = Round(_y[i]),
                    Uij = Round(_u(_x[i], _t[j])),
                    Rij = Round(r)
                });
            }
            _items.Add(new ItemSource
            {
                RMax = Round(_rMax)
            });

            _rGlobal = Math.Max(_rMax, _rGlobal);
        }

        private void SaveGlobalR()
        {
            _items.Add(new ItemSource
            {
                RGlobal = Round(_rGlobal)
            });
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
            }

            SaveLayer(j + 1);
            _y0 = _y;

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
