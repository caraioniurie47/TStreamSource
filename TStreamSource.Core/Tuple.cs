#region LICENSE

//  TStreamSource - MPEG Stream Tools
//  Copyright (C) 2011 MarkTwen (mktwen@gmail.com)
//  http://www.TStreamSource.com

//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 3 of the License, or (at your option) any later version.

//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.

//  You should have received a copy of the GNU Lesser General
//  Public License along with this library; 
//  If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace TStreamSource.Core
{
    #region Usings

    using System;

    #endregion

    /// <summary>
    /// Tuple implementation.
    /// </summary>
    /// <typeparam name="T">The first type.</typeparam>
    /// <typeparam name="U">The second type.</typeparam>
    public struct Tuple<T, U> : IEquatable<Tuple<T, U>>
    {
        #region Properties

        #region First

        private readonly T first;
        
        /// <summary>
        /// The first property.
        /// </summary>
        public T First
        {
            get { return first; }
        }

        #endregion

        #region Second

        private readonly U second;
        
        /// <summary>
        /// The second property.
        /// </summary>
        public U Second
        {
            get { return second; }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="first">The first value.</param>
        /// <param name="second">The second value.</param>
        public Tuple(T first, U second)
        {
            this.first = first;
            this.second = second;
        }

        #endregion

        #region Methods

        #region GetHashCode

        /// <summary>
        /// Compute hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return first.GetHashCode() ^ second.GetHashCode();
        }

        #endregion

        #region Equals Methods

        /// <summary>
        /// Check for equality.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>true if equals, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            #region CHECK

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            #endregion

            return Equals((Tuple<T, U>)obj);
        }

        /// <summary>
        /// Test for equality.
        /// </summary>
        /// <param name="other">The object to check.</param>
        /// <returns>true if equals otherwise false.</returns>
        public bool Equals(Tuple<T, U> other)
        {
            return other.first.Equals(first) && other.second.Equals(second);
        }

        #endregion

        #endregion
    }
}
