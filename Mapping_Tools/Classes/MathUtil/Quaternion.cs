/*
Copyright (c) 2006 - 2008 The Open Toolkit library.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace Mapping_Tools.Classes.MathUtil {
    /// <summary>
    /// Represents a Quaternion.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Quaternion :IEquatable<Quaternion> {
        /// <summary>
        /// The X, Y and Z components of this instance.
        /// </summary>
        public Vector3 Xyz;

        /// <summary>
        /// The W component of this instance.
        /// </summary>
        public double W;

        /// <summary>
        /// Construct a new Quaternion from vector and w components
        /// </summary>
        /// <param name="v">The vector part</param>
        /// <param name="w">The w part</param>
        public Quaternion(Vector3 v, double w) {
            Xyz = v;
            W = w;
        }

        /// <summary>
        /// Construct a new Quaternion
        /// </summary>
        /// <param name="x">The x component</param>
        /// <param name="y">The y component</param>
        /// <param name="z">The z component</param>
        /// <param name="w">The w component</param>
        public Quaternion(double x, double y, double z, double w)
            : this(new Vector3(x, y, z), w) { }

        /// <summary>
        /// Construct a new Quaternion from given Euler angles in radians. 
        /// The rotations will get applied in following order:
        /// 1. around X axis, 2. around Y axis, 3. around Z axis
        /// </summary>
        /// <param name="rotationX">Counterclockwise rotation around X axis in radian</param>
        /// <param name="rotationY">Counterclockwise rotation around Y axis in radian</param>
        /// <param name="rotationZ">Counterclockwise rotation around Z axis in radian</param>
        public Quaternion(double rotationX, double rotationY, double rotationZ) {
            rotationX *= 0.5f;
            rotationY *= 0.5f;
            rotationZ *= 0.5f;

            double c1 = Math.Cos(rotationX);
            double c2 = Math.Cos(rotationY);
            double c3 = Math.Cos(rotationZ);
            double s1 = Math.Sin(rotationX);
            double s2 = Math.Sin(rotationY);
            double s3 = Math.Sin(rotationZ);

            W = c1 * c2 * c3 - s1 * s2 * s3;
            Xyz.X = s1 * c2 * c3 + c1 * s2 * s3;
            Xyz.Y = c1 * s2 * c3 - s1 * c2 * s3;
            Xyz.Z = c1 * c2 * s3 + s1 * s2 * c3;
        }

        /// <summary>
        /// Construct a new Quaternion from given Euler angles. The rotations will get applied in following order:
        /// 1. Around X, 2. Around Y, 3. Around Z
        /// </summary>
        /// <param name="eulerAngles">The counterclockwise euler angles as a Vector3</param>
        public Quaternion(Vector3 eulerAngles)
            : this(eulerAngles.X, eulerAngles.Y, eulerAngles.Z) { }

        /// <summary>
        /// Gets or sets the X component of this instance.
        /// </summary>
        [XmlIgnore]
        public double X { get { return Xyz.X; } set { Xyz.X = value; } }

        /// <summary>
        /// Gets or sets the Y component of this instance.
        /// </summary>
        [XmlIgnore]
        public double Y { get { return Xyz.Y; } set { Xyz.Y = value; } }

        /// <summary>
        /// Gets or sets the Z component of this instance.
        /// </summary>
        [XmlIgnore]
        public double Z { get { return Xyz.Z; } set { Xyz.Z = value; } }

        /// <summary>
        /// Convert the current quaternion to axis angle representation
        /// </summary>
        /// <param name="axis">The resultant axis</param>
        /// <param name="angle">The resultant angle</param>
        public void ToAxisAngle(out Vector3 axis, out double angle) {
            Vector4 result = ToAxisAngle();
            axis = result.Xyz;
            angle = result.W;
        }

        /// <summary>
        /// Convert this instance to an axis-angle representation.
        /// </summary>
        /// <returns>A Vector4 that is the axis-angle representation of this quaternion.</returns>
        public Vector4 ToAxisAngle() {
            Quaternion q = this;
            if( Math.Abs(q.W) > 1.0f ) {
                q.Normalize();
            }

            Vector4 result = new Vector4
            {
                W = 2.0f * System.Math.Acos(q.W) // angle
            };
            double den = System.Math.Sqrt(1.0 - q.W * q.W);
            if( den > 0.0001f ) {
                result.Xyz = q.Xyz / den;
            }
            else {
                // This occurs when the angle is zero.
                // Not a problem: just set an arbitrary normalized axis.
                result.Xyz = Vector3.UnitX;
            }

            return result;
        }

        /// <summary>
        /// Gets the length (magnitude) of the quaternion.
        /// </summary>
        /// <seealso cref="LengthSquared"/>
        public double Length {
            get {
                return System.Math.Sqrt(W * W + Xyz.LengthSquared);
            }
        }

        /// <summary>
        /// Gets the square of the quaternion length (magnitude).
        /// </summary>
        public double LengthSquared {
            get {
                return W * W + Xyz.LengthSquared;
            }
        }

        /// <summary>
        /// Returns a copy of the Quaternion scaled to unit length.
        /// </summary>
        public Quaternion Normalized() {
            Quaternion q = this;
            q.Normalize();
            return q;
        }

        /// <summary>
        /// Reverses the rotation angle of this Quaterniond.
        /// </summary>
        public void Invert() {
            W = -W;
        }

        /// <summary>
        /// Returns a copy of this Quaterniond with its rotation angle reversed.
        /// </summary>
        public Quaternion Inverted() {
            var q = this;
            q.Invert();
            return q;
        }

        /// <summary>
        /// Scales the Quaternion to unit length.
        /// </summary>
        public void Normalize() {
            double scale = 1.0f / this.Length;
            Xyz *= scale;
            W *= scale;
        }

        /// <summary>
        /// Inverts the Vector3 component of this Quaternion.
        /// </summary>
        public void Conjugate() {
            Xyz = -Xyz;
        }

        /// <summary>
        /// Defines the identity quaternion.
        /// </summary>
        public static readonly Quaternion Identity = new Quaternion(0, 0, 0, 1);

        /// <summary>
        /// Add two quaternions
        /// </summary>
        /// <param name="left">The first operand</param>
        /// <param name="right">The second operand</param>
        /// <returns>The result of the addition</returns>
        public static Quaternion Add(Quaternion left, Quaternion right) {
            return new Quaternion(
                left.Xyz + right.Xyz,
                left.W + right.W);
        }

        /// <summary>
        /// Add two quaternions
        /// </summary>
        /// <param name="left">The first operand</param>
        /// <param name="right">The second operand</param>
        /// <param name="result">The result of the addition</param>
        public static void Add(ref Quaternion left, ref Quaternion right, out Quaternion result) {
            result = new Quaternion(
                left.Xyz + right.Xyz,
                left.W + right.W);
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns>The result of the operation.</returns>
        public static Quaternion Sub(Quaternion left, Quaternion right) {
            return new Quaternion(
                left.Xyz - right.Xyz,
                left.W - right.W);
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <param name="result">The result of the operation.</param>
        public static void Sub(ref Quaternion left, ref Quaternion right, out Quaternion result) {
            result = new Quaternion(
                left.Xyz - right.Xyz,
                left.W - right.W);
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static Quaternion Multiply(Quaternion left, Quaternion right) {
            Multiply(ref left, ref right, out Quaternion result);
            return result;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <param name="result">A new instance containing the result of the calculation.</param>
        public static void Multiply(ref Quaternion left, ref Quaternion right, out Quaternion result) {
            result = new Quaternion(
                right.W * left.Xyz + left.W * right.Xyz + Vector3.Cross(left.Xyz, right.Xyz),
                left.W * right.W - Vector3.Dot(left.Xyz, right.Xyz));
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <param name="result">A new instance containing the result of the calculation.</param>
        public static void Multiply(ref Quaternion quaternion, double scale, out Quaternion result) {
            result = new Quaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static Quaternion Multiply(Quaternion quaternion, double scale) {
            return new Quaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
        }

        /// <summary>
        /// Get the conjugate of the given quaternion
        /// </summary>
        /// <param name="q">The quaternion</param>
        /// <returns>The conjugate of the given quaternion</returns>
        public static Quaternion Conjugate(Quaternion q) {
            return new Quaternion(-q.Xyz, q.W);
        }

        /// <summary>
        /// Get the conjugate of the given quaternion
        /// </summary>
        /// <param name="q">The quaternion</param>
        /// <param name="result">The conjugate of the given quaternion</param>
        public static void Conjugate(ref Quaternion q, out Quaternion result) {
            result = new Quaternion(-q.Xyz, q.W);
        }

        /// <summary>
        /// Get the inverse of the given quaternion
        /// </summary>
        /// <param name="q">The quaternion to invert</param>
        /// <returns>The inverse of the given quaternion</returns>
        public static Quaternion Invert(Quaternion q) {
            Invert(ref q, out Quaternion result);
            return result;
        }

        /// <summary>
        /// Get the inverse of the given quaternion
        /// </summary>
        /// <param name="q">The quaternion to invert</param>
        /// <param name="result">The inverse of the given quaternion</param>
        public static void Invert(ref Quaternion q, out Quaternion result) {
            double lengthSq = q.LengthSquared;
            if( lengthSq != 0.0 ) {
                double i = 1.0f / lengthSq;
                result = new Quaternion(q.Xyz * -i, q.W * i);
            }
            else {
                result = q;
            }
        }

        /// <summary>
        /// Scale the given quaternion to unit length
        /// </summary>
        /// <param name="q">The quaternion to normalize</param>
        /// <returns>The normalized quaternion</returns>
        public static Quaternion Normalize(Quaternion q) {
            Normalize(ref q, out Quaternion result);
            return result;
        }

        /// <summary>
        /// Scale the given quaternion to unit length
        /// </summary>
        /// <param name="q">The quaternion to normalize</param>
        /// <param name="result">The normalized quaternion</param>
        public static void Normalize(ref Quaternion q, out Quaternion result) {
            double scale = 1.0f / q.Length;
            result = new Quaternion(q.Xyz * scale, q.W * scale);
        }

        /// <summary>
        /// Build a quaternion from the given axis and angle in radians
        /// </summary>
        /// <param name="axis">The axis to rotate about</param>
        /// <param name="angle">The rotation angle in radians</param>
        /// <returns>The equivalent quaternion</returns>
        public static Quaternion FromAxisAngle(Vector3 axis, double angle) {
            if( axis.LengthSquared == 0.0f ) {
                return Identity;
            }

            Quaternion result = Identity;

            angle *= 0.5f;
            axis.Normalize();
            result.Xyz = axis * System.Math.Sin(angle);
            result.W = System.Math.Cos(angle);

            return Normalize(result);
        }

        /// <summary>
        /// Builds a Quaternion from the given euler angles in radians
        /// The rotations will get applied in following order:
        /// 1. pitch (X axis), 2. yaw (Y axis), 3. roll (Z axis)
        /// </summary>
        /// <param name="pitch">The pitch (attitude), counterclockwise rotation around X axis</param>
        /// <param name="yaw">The yaw (heading), counterclockwise rotation around Y axis</param>
        /// <param name="roll">The roll (bank), counterclockwise rotation around Z axis</param>
        /// <returns></returns>
        public static Quaternion FromEulerAngles(double pitch, double yaw, double roll) {
            return new Quaternion(pitch, yaw, roll);
        }

        /// <summary>
        /// Builds a Quaternion from the given euler angles in radians.
        /// The rotations will get applied in following order:
        /// 1. X axis, 2. Y axis, 3. Z axis
        /// </summary>
        /// <param name="eulerAngles">The counterclockwise euler angles as a vector</param>
        /// <returns>The equivalent Quaternion</returns>
        public static Quaternion FromEulerAngles(Vector3 eulerAngles) {
            return new Quaternion(eulerAngles);
        }

        /// <summary>
        /// Builds a Quaternion from the given euler angles in radians.
        /// The rotations will get applied in following order:
        /// 1. Around X, 2. Around Y, 3. Around Z
        /// </summary>
        /// <param name="eulerAngles">The counterclockwise euler angles a vector</param>
        /// <param name="result">The equivalent Quaternion</param>
        public static void FromEulerAngles(ref Vector3 eulerAngles, out Quaternion result) {

            double c1 = Math.Cos(eulerAngles.X * 0.5f);
            double c2 = Math.Cos(eulerAngles.Y * 0.5f);
            double c3 = Math.Cos(eulerAngles.Z * 0.5f);
            double s1 = Math.Sin(eulerAngles.X * 0.5f);
            double s2 = Math.Sin(eulerAngles.Y * 0.5f);
            double s3 = Math.Sin(eulerAngles.Z * 0.5f);

            result.W = c1 * c2 * c3 - s1 * s2 * s3;
            result.Xyz.X = s1 * c2 * c3 + c1 * s2 * s3;
            result.Xyz.Y = c1 * s2 * c3 - s1 * c2 * s3;
            result.Xyz.Z = c1 * c2 * s3 + s1 * s2 * c3;
        }

        /// <summary>
        /// Builds a quaternion from the given rotation matrix
        /// </summary>
        /// <param name="matrix">A rotation matrix</param>
        /// <returns>The equivalent quaternion</returns>
        public static Quaternion FromMatrix(Matrix3 matrix) {
            FromMatrix(ref matrix, out Quaternion result);
            return result;
        }

        /// <summary>
        /// Builds a quaternion from the given rotation matrix
        /// </summary>
        /// <param name="matrix">A rotation matrix</param>
        /// <param name="result">The equivalent quaternion</param>
        public static void FromMatrix(ref Matrix3 matrix, out Quaternion result) {
            double trace = matrix.Trace;

            if( trace > 0 ) {
                double s = Math.Sqrt(trace + 1) * 2;
                double invS = 1f / s;

                result.W = s * 0.25f;
                result.Xyz.X = ( matrix.Row2.Y - matrix.Row1.Z ) * invS;
                result.Xyz.Y = ( matrix.Row0.Z - matrix.Row2.X ) * invS;
                result.Xyz.Z = ( matrix.Row1.X - matrix.Row0.Y ) * invS;
            }
            else {
                double m00 = matrix.Row0.X, m11 = matrix.Row1.Y, m22 = matrix.Row2.Z;

                if( m00 > m11 && m00 > m22 ) {
                    double s = Math.Sqrt(1 + m00 - m11 - m22) * 2;
                    double invS = 1f / s;

                    result.W = ( matrix.Row2.Y - matrix.Row1.Z ) * invS;
                    result.Xyz.X = s * 0.25f;
                    result.Xyz.Y = ( matrix.Row0.Y + matrix.Row1.X ) * invS;
                    result.Xyz.Z = ( matrix.Row0.Z + matrix.Row2.X ) * invS;
                }
                else if( m11 > m22 ) {
                    double s = Math.Sqrt(1 + m11 - m00 - m22) * 2;
                    double invS = 1f / s;

                    result.W = ( matrix.Row0.Z - matrix.Row2.X ) * invS;
                    result.Xyz.X = ( matrix.Row0.Y + matrix.Row1.X ) * invS;
                    result.Xyz.Y = s * 0.25f;
                    result.Xyz.Z = ( matrix.Row1.Z + matrix.Row2.Y ) * invS;
                }
                else {
                    double s = Math.Sqrt(1 + m22 - m00 - m11) * 2;
                    double invS = 1f / s;

                    result.W = ( matrix.Row1.X - matrix.Row0.Y ) * invS;
                    result.Xyz.X = ( matrix.Row0.Z + matrix.Row2.X ) * invS;
                    result.Xyz.Y = ( matrix.Row1.Z + matrix.Row2.Y ) * invS;
                    result.Xyz.Z = s * 0.25f;
                }
            }
        }

        /// <summary>
        /// Do Spherical linear interpolation between two quaternions
        /// </summary>
        /// <param name="q1">The first quaternion</param>
        /// <param name="q2">The second quaternion</param>
        /// <param name="blend">The blend factor</param>
        /// <returns>A smooth blend between the given quaternions</returns>
        public static Quaternion Slerp(Quaternion q1, Quaternion q2, double blend) {
            // if either input is zero, return the other.
            if( q1.LengthSquared == 0.0f ) {
                if( q2.LengthSquared == 0.0f ) {
                    return Identity;
                }
                return q2;
            }
            else if( q2.LengthSquared == 0.0f ) {
                return q1;
            }


            double cosHalfAngle = q1.W * q2.W + Vector3.Dot(q1.Xyz, q2.Xyz);

            if( cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f ) {
                // angle = 0.0f, so just return one input.
                return q1;
            }
            else if( cosHalfAngle < 0.0f ) {
                q2.Xyz = -q2.Xyz;
                q2.W = -q2.W;
                cosHalfAngle = -cosHalfAngle;
            }

            double blendA;
            double blendB;
            if( cosHalfAngle < 0.99f ) {
                // do proper slerp for big angles
                double halfAngle = System.Math.Acos(cosHalfAngle);
                double sinHalfAngle = System.Math.Sin(halfAngle);
                double oneOverSinHalfAngle = 1.0f / sinHalfAngle;
                blendA = System.Math.Sin(halfAngle * (1.0f - blend)) * oneOverSinHalfAngle;
                blendB = System.Math.Sin(halfAngle * blend) * oneOverSinHalfAngle;
            }
            else {
                // do lerp if angle is really small.
                blendA = 1.0f - blend;
                blendB = blend;
            }

            Quaternion result = new Quaternion(blendA * q1.Xyz + blendB * q2.Xyz, blendA * q1.W + blendB * q2.W);
            if( result.LengthSquared > 0.0f ) {
                return Normalize(result);
            }
            else {
                return Identity;
            }
        }

        /// <summary>
        /// Adds two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Quaternion operator +(Quaternion left, Quaternion right) {
            left.Xyz += right.Xyz;
            left.W += right.W;
            return left;
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Quaternion operator -(Quaternion left, Quaternion right) {
            left.Xyz -= right.Xyz;
            left.W -= right.W;
            return left;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Quaternion operator *(Quaternion left, Quaternion right) {
            Multiply(ref left, ref right, out left);
            return left;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static Quaternion operator *(Quaternion quaternion, double scale) {
            Multiply(ref quaternion, scale, out quaternion);
            return quaternion;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static Quaternion operator *(double scale, Quaternion quaternion) {
            return new Quaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(Quaternion left, Quaternion right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equal right; false otherwise.</returns>
        public static bool operator !=(Quaternion left, Quaternion right) {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a System.string that represents the current Quaternion.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return string.Format("V: {0}, W: {1}", Xyz, W);
        }

        /// <summary>
        /// Compares this object instance to another object for equality.
        /// </summary>
        /// <param name="other">The other object to be used in the comparison.</param>
        /// <returns>True if both objects are Quaternions of equal value. Otherwise it returns false.</returns>
        public override bool Equals(object? other) {
            if( other is Quaternion == false ) {
                return false;
            }
            return this == (Quaternion) other;
        }

        /// <summary>
        /// Provides the hash code for this object.
        /// </summary>
        /// <returns>A hash code formed from the bitwise XOR of this objects members.</returns>
        public override int GetHashCode() {
            unchecked {
                return ( this.Xyz.GetHashCode() * 397 ) ^ this.W.GetHashCode();
            }
        }

        /// <summary>
        /// Compares this Quaternion instance to another Quaternion for equality.
        /// </summary>
        /// <param name="other">The other Quaternion to be used in the comparison.</param>
        /// <returns>True if both instances are equal; false otherwise.</returns>
        public bool Equals(Quaternion other) {
            return Xyz == other.Xyz && W == other.W;
        }
    }
}
