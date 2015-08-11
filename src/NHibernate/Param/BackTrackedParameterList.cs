using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.SqlCommand;

namespace NHibernate.Param
{
    public class BackTrackedParameterList: IList<Parameter>
    {
        private readonly IDictionary<object, Tuple<Parameter, int>> _dictionary;

        private readonly IList<Parameter> _underlyingList;

        public BackTrackedParameterList(IEnumerable<Parameter> sqlParameters)
        {
            _underlyingList = sqlParameters.ToList();
            _dictionary = _underlyingList
                .Select((p, i) => new Tuple<Parameter, int>(p, i))
                .ToDictionary(pair => pair.Item1.BackTrack);
        }

        public bool IndexOfBackTrack(string backTrackId, out int index)
        {
            Tuple<Parameter, int> pair;
            if (_dictionary.TryGetValue(backTrackId, out pair))
            {
                index = pair.Item2;
                return true;
            }
            else
            {
                index = -1;
                return false;
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
            _dictionary.Clear();
        }

        bool ICollection<Parameter>.Contains(Parameter item)
        {
            return _dictionary.ContainsKey(item.BackTrack);
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
            get { return _dictionary.Count; }
        }

        bool ICollection<Parameter>.IsReadOnly
        {
            get { return true; }
        }

        int IList<Parameter>.IndexOf(Parameter item)
        {
            int index;
            IndexOfBackTrack(item.BackTrack.ToString(), out index);
            return index;
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