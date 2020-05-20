using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WelcomeToVichMat
{
    // Построение таблицы вторых производних с 1 типом краевых условий
    class CubicSpline
    {
        private int n;
        private List<double> x, y;
        private double A, B;
        private Dictionary<double, double> SecondDerivative;

        private double[] a, b, c, d, mu, nu;

        double h(int i) => x[i] - x[i - 1];
        double F(int i) => (y[i + 1] - y[i]) / h(i + 1) - (y[i] - y[i - 1]) / h(i);

        /// <summary>
        /// Вторая производная сплайна.
        /// </summary>
        /// <param name="i">номер промежутка</param>
        /// <param name="xx">точка, в которой надо вычислить</param>
        /// <returns></returns>
        double S_derivative_2(int i, double xx) => c[i] + d[i] * (xx - x[i]);

        public CubicSpline()
        {

        }

        /// <summary>
        /// Начало решения.
        /// </summary>
        /// <param name="filePath"></param>
        void Start(string filePath)
        {
            if (ReadData(filePath))
            {
                if (CheckData() != Error.None)
                {
                    CalculateSpline();
                    SecondDerivative = GetListSecondDerivative();
                    WriteData("output.txt");
                }
            }
            else
            {
                ShowError(Error.CanNotReadTheFile);
            }
        }

        /// <summary>
        /// Считать данные с файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns></returns>
        bool ReadData(string filePath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(filePath);
            //if (!dirInfo.Exists)
            //return false;

            using (StreamReader stream = new StreamReader(filePath))
            {
                // 5 строк в файле
                try
                {
                    this.n = int.Parse(stream.ReadLine());

                    this.x = stream.ReadLine()
                        .Split(" ".ToCharArray(), n, options: StringSplitOptions.RemoveEmptyEntries)
                        .Select(itm => double.Parse(itm)).ToList();

                    this.y = stream.ReadLine()
                        .Split(" ".ToCharArray(), n, options: StringSplitOptions.RemoveEmptyEntries)
                         .Select(itm => double.Parse(itm)).ToList();

                    this.A = int.Parse(stream.ReadLine());

                    this.B = int.Parse(stream.ReadLine());
                }
                catch (Exception e)
                {
                    return false;
                }

                a = new double[n];
                b = new double[n];
                c = new double[n];
                d = new double[n];
                mu = new double[n];
                nu = new double[n];

                return true;
            }

        }

        /// <summary>
        /// Показать ошибку на экран.
        /// </summary>
        /// <param name="IOR">Номер ошибки.</param>
        void ShowError(Error IOR = Error.None)
        {
            switch (IOR)
            {
                case Error.None:
                    Console.WriteLine("Неизвестная ошибка");
                    break;
                case Error.NotEnoughPoints:
                    Console.WriteLine("Недостаточно точек для построения сплайна");
                    break;
                case Error.ViolationOfOrder:
                    Console.WriteLine("Нарушен порядое точек");
                    break;
                case Error.CanNotReadTheFile:
                    Console.WriteLine("Не возможно прочитать файл");
                    break;
                case Error.IncorrectData:
                    Console.WriteLine("Не верные данные");
                    break;
            }
        }

        /// <summary>
        /// Проверка данных на ошибки.
        /// </summary>
        /// <returns></returns>
        Error CheckData()
        {
            if (this.n < 3)
                return Error.NotEnoughPoints;

            if (this.x.Count < this.n || this.y.Count < this.n)
                return Error.IncorrectData;

            for (int i = 1; i < n; i++)
            {
                if (x[i] >= x[i - 1])
                    return Error.ViolationOfOrder;
            }
            return Error.None;
        }

        /// <summary>
        /// Расчет сплайна
        /// </summary>
        void CalculateSpline()
        {
            mu[1] = 0;
            nu[1] = A;

            for (int i = 1; i < n; i++)
            {
                mu[i + 1] = -h(i + 1) / (2 * (h(i) + h(i + 1)) + h(i) * mu[i]);
                nu[i + 1] = (6 * F(i) - h(i) * nu[i]) / (2 * (h(i) + h(i + 1)) + h(i) * mu[i]);
            }

            for (int i = n - 1; i > 0; i--)
            {
                c[i] = mu[i + 1] * c[i + 1] + nu[i + 1];
            }

            for (int i = 0; i < n + 1; i++)
                a[i] = y[i];

            for (int i = 1; i < n + 1; i++)
            {
                d[i] = (c[i] - c[i - 1]) / h(i);
            }

            for (int i = 1; i < n + 1; i++)
            {
                b[i] = (h(i) / 2) * c[i] - (h(i) * h(i) / 6) * d[i] + (y[i] - y[i - 1]) / h(i);
            }
        }

        /// <summary>
        /// Выбор краевых условий.
        /// </summary>
        /// <param name="number"></param>
        void SetBoundaryCondition(int number = 0)
        {
            switch (number)
            {
                case 1:
                    c[0] = A;
                    c[n - 1] = B;
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                default:
                    c[0] = 0;
                    c[n - 1] = 0;
                    break;
            }
        }

        /// <summary>
        /// Получение списка вторых производных.
        /// </summary>
        /// <returns></returns>
        Dictionary<double, double> GetListSecondDerivative()
        {
            var dict = new Dictionary<double, double>();

            for(int i = 0; i < n; i++)
            {
                dict.Add(x[i], S_derivative_2(i, x[i]));
            }

            return dict;
        }

        /// <summary>
        /// Запись даннных в файл.
        /// </summary>
        /// <param name="filePath">Имя файла.</param>
        /// <returns></returns>
        bool WriteData(string filePath)
        {
            using (StreamWriter stream = new StreamWriter(filePath))
            {
                foreach (var touple in SecondDerivative)
                {
                    stream.WriteLine("x: {0}, y: {1}", touple.Key, touple.Value);
                }
            }
            return true;
        }
    }
}
