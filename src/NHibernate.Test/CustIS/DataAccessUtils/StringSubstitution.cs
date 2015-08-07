using System;

namespace NHibernate.Test.CustIS.DataAccessUtils
{
    /// <summary> 
    /// Информация о замене подстроки в строке: с какого индекса
    /// начинать, каким индексом заканчивать, какую строку подставлять. 
    /// </summary>
    [Serializable]
    public class StringSubstitution
    {
        /// <summary> Индекс первого символа заменяемой подстроки. </summary>
        private readonly int _fromInclusive;

        /// <summary> Индекс первого символа, следующего после заменяемой подстроки. </summary>
        private readonly int _toExclusive;

        /// <summary> Новая строка, которая вставляется вместо заменяемой подстроки. </summary>
        private readonly string _newChunk;

        /// <summary> 
        /// Информация о замене подстроки в строке: с какого индекса
        /// начинать, каким индексом заканчивать, какую строку подставлять. 
        /// </summary>
        /// <param name="fromInc">
        ///     Индекс, начиная с которого начинать подстановку, включительно.
        ///     В результирующей строке данного символа не будет.
        /// </param>
        /// <param name="toExc">
        ///     Индекс символа в исходной строки, который будет следовать первым
        ///     после подставноки в новой строке.
        /// </param>
        /// <param name="newChunk">
        ///     Строка, которая будет вставлена между символами <paramref name="fromInc"/>
        ///     и <paramref name="toExc"/>.
        /// </param>
        public StringSubstitution(int fromInc, int toExc, string newChunk)
        {
            _fromInclusive = fromInc;
            _toExclusive = toExc;
            _newChunk = newChunk;
        }

        /// <summary>  Применить подстановку. </summary>
        public string Apply(string source)
        {
            return source.Substring(0, _fromInclusive)
                   + _newChunk
                   + source.Substring(_toExclusive);
        }
    }
}