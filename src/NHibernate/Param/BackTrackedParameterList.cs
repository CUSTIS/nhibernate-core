using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.SqlCommand;

namespace NHibernate.Param
{
    internal class BackTrackedParameterList: IList<Parameter>
    {
        private readonly IDictionary<object, Tuple<Parameter, int>[]> _dictionary;

        private readonly IList<Parameter> _underlyingList;

        public BackTrackedParameterList(IEnumerable<Parameter> sqlParameters)
        {
            _underlyingList = sqlParameters.ToList();
            _dictionary = _underlyingList
                .Select((p, i) => new Tuple<Parameter, int>(p, i))
                .GroupBy(pair => pair.Item1.BackTrack)
                .ToDictionary(gr => gr.Key, gr => gr.ToArray());
        }

        public IEnumerable<int> GetBackTrackIndeces(string backTrackId)
        {
            Tuple<Parameter, int>[] indeces;
            if (_dictionary.TryGetValue(backTrackId, out indeces))
            {
                return indeces.Select(pair => pair.Item2);
            }
            else
            {
                return Enumerable.Empty<int>();
            }
        }

        #region IList<Parameter> implementation 

        IEnumerator<Parameter> IEnumerable<Parameter>.GetEnumerator()
        {
            return _underlyingList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Parameter>)this).GetEnumerator();
        }

        void ICollection<Parameter>.Add(Parameter item)
        {
            throw new NotSupportedException();
        }

        void ICollection<Parameter>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<Parameter>.Contains(Parameter item)
        {
            return _underlyingList.Contains(item);
        }

        public void CopyTo(Parameter[] array, int arrayIndex)
        {
            _underlyingList.CopyTo(array, arrayIndex);
        }

        bool ICollection<Parameter>.Remove(Parameter item)
        {
            throw new NotSupportedException();
        }

        int ICollection<Parameter>.Count
        {
            get { return _underlyingList.Count; }
        }

        bool ICollection<Parameter>.IsReadOnly
        {
            get { return true; }
        }

        int IList<Parameter>.IndexOf(Parameter item)
        {
            return _underlyingList.IndexOf(item);
        }

        void IList<Parameter>.Insert(int index, Parameter item)
        {
            throw new NotSupportedException();
        }

        void IList<Parameter>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public Parameter this[int index]
        {
            get { return _underlyingList[index]; }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion
    }
}