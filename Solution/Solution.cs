using System;
using System.Collections.Generic;
using System.IO;
using WelcomeToVichMat;

namespace VichMat.Solution
{
    public class Solution
    {
        private readonly MainWindow _window;
        private Action<StreamReader> _reader;
        private Action<StreamWriter> _writer;
        private Action _solve;
        private Func<(Error error, string data)> _checking;

        private (Error error, string data) _error;

        public Solution()
        {
            _window = MainWindow.GetWindow();
        }

        public Solution(
            Action<StreamReader> reader,
            Action<StreamWriter> writer,
            Func<(Error error, string data)> checking, Action solve) : this()
        {
            _reader = reader;
            _writer = writer;
            _checking = checking;
            _solve = solve;
        }

        public Solution(ILaba laba) : this(laba.ReadData, laba.WriteData, laba.CheckData, laba.Solve)
        {
        }

        /// <summary>
        /// Начало решения.
        /// </summary>
        /// <param name="inputFilePath">Путь к файлу ввода данных.</param>
        /// <param name="outputFilePath">Путь к файлу вывода.</param>
        public void Start(string inputFilePath = "input.txt", string outputFilePath = "output.txt")
        {
            ReadData(inputFilePath, _reader);
            CheckData(_checking);
            Solve();
            WriteData(outputFilePath, _writer);
        }

        /// <summary>
        /// Считать данные с файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="reader">Правило чтения файла.</param>
        /// <returns></returns>
        private void ReadData(string filePath, Action<StreamReader> reader)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                SetError(Error.RootsNotExists);
            }

            using (StreamReader stream = new StreamReader(filePath))
            {
                try
                {
                    reader.Invoke(stream);
                }
                catch (Exception e)
                {
                    SetError(Error.CanNotReadTheFile, e.Message);
                }
            }

        }

        /// <summary>
        /// Проверка данных на ошибки.
        /// </summary>
        /// <param name="checking">Правило проверки.</param>
        private void CheckData(Func<(Error error, string data)> checking)
        {
            var (error, data) = checking();
            if (error != Error.None)
            {
                SetError(error, data);
            }
        }

        /// <summary>
        /// Запись данных в файл.
        /// </summary>
        /// <param name="filePath">Имя файла.</param>
        /// <param name="writer">Правило записи в файл.</param>
        private void WriteData(string filePath, Action<StreamWriter> writer)
        {
            using (var stream = new StreamWriter(filePath))
            {
                try
                {
                    writer.Invoke(stream);
                }
                catch (Exception e)
                {
                    SetError(Error.CanNotWriteTheFile, e.Message);
                }
            }
        }

        /// <summary>
        /// Показать ошибку на экран.
        /// </summary>
        private void ShowInfo(bool close = true)
        {
            switch (_error.error)
            {
                case Error.Unknown:
                    _window.ShowError("Неизвестная ошибка", close);
                    break;
                case Error.CanNotReadTheFile:
                    _window.ShowError("Ошибка при чтении файла", close);
                    break;
                case Error.CanNotWriteTheFile:
                    _window.ShowError("Ошибка при записи в файл", close);
                    break;
                case Error.DivisionByZero:
                    _window.ShowError("Произошло деление на ноль :(", close);
                    break;
                case Error.MaxIteration:
                    _window.ShowError($"Превышено максимальное количество итераций {_error.data}", close);
                    break;
                case Error.FuncIsNotDefined:
                    _window.ShowError($"Произошел выход за границу определения функции", close);
                    break;
                case Error.RootsNotExists:
                    _window.ShowError($"Папка не существует", close);
                    break;
                case Error.Custom:
                    _window.ShowError(_error.data, close);
                    break;
                case Error.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetError(Error error, string data = null, bool isInformation = false)
        {
            _error.error = error;
            _error.data = data;
            ShowInfo(!isInformation);
        }

        public void Solve()
        {
            try
            {
                _solve.Invoke();
            }
            catch (DivideByZeroException e)
            {
                SetError(Error.DivisionByZero, e.Message);
            }
            catch (Exception e)
            {
                SetError(Error.Unknown, e.Message);
            }
        }

        public void ShowTable<TSource>(List<TSource> items)
        {
            _window.CreateTable(items);
        }

        public void ShowData(string data)
        {
            _window.ShowData(data);
        }
    }
}
