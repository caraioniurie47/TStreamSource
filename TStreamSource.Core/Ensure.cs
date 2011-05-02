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
    using System.Globalization;

    using NLog;

    #endregion

    /// <summary>
    /// Validation methods.
    /// TODO: Write specific methods for performance?
    /// This class is inspired from NUnit, and is Public Domain.
    /// </summary>
    public static class Ensure
    {
        #region AreEqual

        public static void AreEqual<T>(Logger log, T expected, T actual, string message)
        {
            if (expected.Equals(actual)) return;
            var msg = string.Format(CultureInfo.InvariantCulture, 
                "AreEqual, {0}, expected: {1}, actual: {2}", message, expected, actual);
            ThrowArg(log, msg);
        }

        #endregion

        #region AreNotEqual

        public static void AreNotEqual<T>(Logger log, T expected, T actual, string message)
        {
            if (!expected.Equals(actual)) return;
            var msg = string.Format(CultureInfo.InvariantCulture, 
                "AreNotEqual, {0}, expected: {1}, actual: {2}", message, expected, actual);
            ThrowArg(log, msg);
        }

        #endregion

        #region Greater

        public static void Greater(Logger log, int arg1, int arg2, string message)
        {
            if (arg1 > arg2) return;

            var msg = string.Format(CultureInfo.InvariantCulture, 
                "Greater, {0}, arg1: {1}, arg2: {2}", message, arg1, arg2);
            ThrowArg(log, msg);
        }

        #endregion
        
        #region GreaterZero

        public static void GreaterZero(Logger log, int arg, string message)
        {
            if (arg > 0) return;

            var msg = string.Format(CultureInfo.InvariantCulture, 
                "GreaterZero, {0}, arg: {1}", message, arg);
            ThrowArg(log, msg);
        }
        
        #endregion

        #region GreaterOrEqual

        public static void GreaterOrEqual(Logger log, int arg1, int arg2, string message)
        {
            if (arg1 >= arg2) return;

            var msg = string.Format(CultureInfo.InvariantCulture, 
                "GreaterOrEqual, {0}, arg1: {1}, arg2: {2}", message, arg1, arg2);
            ThrowArg(log, msg);
        }

        #endregion

        #region GreaterOrEqualZero

        public static void GreaterOrEqualZero(Logger log, long arg, string message)
        {
            if (arg >= 0) return;

            var msg = string.Format(CultureInfo.InvariantCulture, 
                "GreaterOrEqualZero, {0}, arg: {1}", message, arg);
            ThrowArg(log, msg);
        }

        #endregion

        #region Less

        public static void Less(Logger log, int arg1, int arg2, string message)
        {
            if (arg1 < arg2) return;

            var msg = string.Format(CultureInfo.InvariantCulture, 
                "Less, {0}, arg1: {1}, arg2: {2}",
                message, arg1, arg2);
            ThrowArg(log, msg);
        }

        #endregion

        #region LessOrEqual

        public static void LessOrEqual(Logger log, long arg1, long arg2, string message)
        {
            if (arg1 <= arg2) return;

            var msg = string.Format(CultureInfo.InvariantCulture, 
                "LessOrEqual, {0}, arg1: {1}, arg2: {2}",
                message, arg1, arg2);
            ThrowArg(log, msg);
        }

        #endregion
       
        #region IsInRange Methods

        // TODO: DRY!
        public static void IsInRange(Logger log, int left, int right, int value, string message)
        {
            if (value >= left && value <= right) return;

            var msg = string.Format(CultureInfo.InvariantCulture, 
                "IsInRange, {0}, [{1}, {2}], value: {3}", message, left, right, value);
            ThrowOutOfRange(log, msg);
        }

        // TODO: DRY!
        public static void IsInRange(Logger log, long left, long right, long value, string message)
        {
            if (value >= left && value <= right) return;

            var msg = string.Format(CultureInfo.InvariantCulture,
                "IsInRange, {0}, [{1}, {2}], value: {3}", message, left, right, value);
            ThrowOutOfRange(log, msg);
        }

        #endregion

        #region IsTrue

        public static void IsTrue(Logger log, bool condition, string message)
        {
            if (condition) return;

            var msg = string.Format(CultureInfo.InvariantCulture, 
                "IsTrue, {0}, condition: {1}",
                message, condition);
            ThrowArg(log, msg);
        }

        #endregion

        #region IsFalse

        public static void IsFalse(Logger log, bool condition, string message)
        {
            if (!condition) return;

            var msg = string.Format(CultureInfo.InvariantCulture, 
                "IsFalse, {0}, condition: {1}", message, condition);
            ThrowArg(log, msg);
        }

        #endregion

        #region IsNull

        public static void IsNull<T>(Logger log, T anObject, string message)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (anObject == null) return;
            // ReSharper restore CompareNonConstrainedGenericWithNull

            var msg = string.Format(CultureInfo.InvariantCulture, 
                "IsNull, {0}, anObject: {1}", message, anObject);
            ThrowNull(log, msg);
        }

        #endregion

        #region IsNotNull

        public static T IsNotNull<T>(Logger log, T anObject, string message)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (anObject == null)
            // ReSharper restore CompareNonConstrainedGenericWithNull
            {
                var msg = string.Format(CultureInfo.InvariantCulture, 
                    "IsNotNull, {0}, anObject: {1}", message, anObject);
                ThrowNull(log, msg);
            }

            return anObject;
        }

        #endregion

        #region IsNotNullOrEmpty

        public static string IsNotNullOrEmpty(Logger log, string anObject, string message)
        {
            if (string.IsNullOrEmpty(anObject))
            {
                var msg = string.Format(CultureInfo.InvariantCulture,
                    "IsNotNullOrEmpty, {0}, anObject: {1}", message, anObject);
                ThrowArg(log, msg);
            }

            return anObject;
        }

        #endregion

        #region IsZero

        public static void IsZero(Logger log, long value, string message)
        {
            if (value == 0) return;

            var msg = string.Format(CultureInfo.InvariantCulture, 
                "IsZero, {0}, value: {1}", message, value);
            ThrowArg(log, msg);
        }

        #endregion

        #region IsNotZero

        public static void IsNotZero(Logger log, long value, string message)
        {
            if (value != 0) return;

            var msg = string.Format(CultureInfo.InvariantCulture, 
                "IsNotZero, {0}, value: {1}", message, value);
            ThrowArg(log, msg);
        }

        #endregion

        #region Fault

        public static Exception Fault(Logger log, bool faultTolerance, string message)
        {
            var msg = string.Format(CultureInfo.InvariantCulture, "Fault, {0}", message);

            var exception = new CustomException(message);
            if (!faultTolerance)
            {
                log.Fatal(CultureInfo.InvariantCulture, msg);
                throw exception;
            }

            log.Error(CultureInfo.InvariantCulture, msg);
            return exception;
        }

        #endregion

        #region Throw

        // TODO: DRY?
        public static void Throw(Logger log, string message)
        {
            var msg = string.Format(CultureInfo.InvariantCulture, "Throw, {0}", message);
            log.Fatal(CultureInfo.InvariantCulture, msg);
            throw new CustomException(message);
        }

        // TODO: DRY?
        public static T Throw<T>(Logger log, string message)
        {
            var msg = string.Format(CultureInfo.InvariantCulture, "Throw, {0}", message);
            log.Fatal(CultureInfo.InvariantCulture, msg);
            throw new CustomException(message);
        }

        #endregion

        #region ThrowArg

        public static void ThrowArg(Logger log, string message)
        {
            log.Fatal(CultureInfo.InvariantCulture, message);
            throw new ArgumentException(message);
        }

        #endregion

        #region ThrowNull

        public static void ThrowNull(Logger log, string message)
        {
            log.Fatal(CultureInfo.InvariantCulture, message);
            throw new ArgumentNullException(message);
        }

        #endregion

        #region ThrowOutOfRange

        public static void ThrowOutOfRange(Logger log, string message)
        {
            log.Fatal(CultureInfo.InvariantCulture, message);
            throw new ArgumentOutOfRangeException(message);
        }

        #endregion
    }
}