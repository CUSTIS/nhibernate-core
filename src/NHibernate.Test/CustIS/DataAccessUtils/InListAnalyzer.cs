using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NHibernate.Test.CustIS.DataAccessUtils
{
    /// <summary> Анализатор IN-выражений в SQL-запросах. </summary>
    internal class InListAnalyzer
    {
        private const string SQL_FOR_ARRAY_FORMAT_WITH_PARAMETER_NAME = "SELECT /*+ cardinality(t {1}) */ COLUMN_VALUE FROM TABLE({0})";
        private const string ARRAY_BIND_PARAMETER_FORMAT_WITH_INDEX = "$array_bind_{0}$";

        /// <summary> Построение информации об оптимизации запроса. </summary>
        /// <remarks> 
        /// По представленному SQL-запросу получить объект, содержащий информацию об 
        /// оптимизациях, применение которых улучшит запрос. При отсутствии IN-выражений,
        /// результирующий объект может и не содержать реальных оптимизаций.
        /// </remarks>
        public QueryCommandMatchingInfo BuildOptimizationInfo(string sql)
        {
            // Планируемые замены в исходном SQL
            var replacements = new List<StringSubstitution>();

            // Диапазоны заменяемых переменных
            var ranges = new List<ParamMatchingInfo>();
            var lastStatement = 0;
            var foundIndex = 0;

            // Поиск проходит таким образом: в цикле находим IN и анализируем его. Запоминаем
            // позицию найденного IN. Следующий поиск будет вестись, начиная со следующей 
            // позиции после найденного IN.
            while (true)
            {
                // Находим ближайший IN, начиная с той части SQL, которая ещё не анализировалась...
                var beginningOfInStatement = sql.IndexOf(" in ", lastStatement, StringComparison.CurrentCultureIgnoreCase);
                if (beginningOfInStatement == -1)
                {
                    break;
                }

                lastStatement = beginningOfInStatement + 1;

                // Для тех IN, которые мы можем оптимизировать,
                // после IN будет идти последовательность в формате "(:bind1{, :bindN})". Если будет 
                // любое расхождение с эталонным форматом, кроме имен Bind-переменных, то считается, что
                // этот IN мы оптимизировать не можем и пропускаем его.

                // Находим предположительно открывающую и закрывающую скобки.
                var openingBracket = sql.IndexOf('(', beginningOfInStatement);
                var closingBracket = sql.IndexOf(')', beginningOfInStatement);

                // Находим предположительный список Bind-переменных.
                var csvBinds = sql.Substring(openingBracket + 1, closingBracket - openingBracket - 1);
                var bindArray =
                    csvBinds
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToArray();

                // Проверка: все аргументы в CSV-перечислении должны быть Bind-переменными (:literal)
                if (!bindArray.All(IsBindedParameter))
                {
                    continue;
                }

                // Все проверки пройдены. Добавляем в результат новый список заменяемых Bind-переменных
                var newBindVariable = string.Format(ARRAY_BIND_PARAMETER_FORMAT_WITH_INDEX, ++foundIndex);
                ranges.Add(new ParamMatchingInfo(bindArray.First(), bindArray.Last(), newBindVariable));

                //... и добавляем в план, какую часть SQL-запроса нам надо подменить своей.
                var newChunk = string.Format(SQL_FOR_ARRAY_FORMAT_WITH_PARAMETER_NAME,
                    string.Format(":{0}", newBindVariable),
                    ComputeCardinality(bindArray.Length));
                var replacement = new StringSubstitution(openingBracket + 1, closingBracket, newChunk);
                replacements.Add(replacement);
            }

            // Выполняем все запланированные подстановки в SQL.
            // В обратном порядке, чтобы не производить переиндексацию.
            foreach (var replacement in Enumerable.Reverse(replacements))
            {
                sql = replacement.Apply(sql);
            }
            return new QueryCommandMatchingInfo(sql, ranges);
        }

        /// <summary> Проверить, является ли <paramref name="parameter"/> именем Bind-переменной. </summary>
        private bool IsBindedParameter(string parameter)
        {
            return Regex.IsMatch(parameter, "^:[a-zA-Z_0-9]*$");
        }

        /// <summary> Вычислить кардинальность. См. bug 133346 </summary>
        private static int ComputeCardinality(int count)
        {
            var pow = Math.Floor(Math.Max(Math.Log10(count) - 0.5, 0));
            var cardinality = Convert.ToInt32(Math.Pow(10, pow));
            return cardinality > 0 ? cardinality : 1;
        }
    }
}