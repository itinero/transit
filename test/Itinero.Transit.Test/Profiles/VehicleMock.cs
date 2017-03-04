// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Itinero.Attributes;
using Itinero.Profiles;
using System;

namespace Itinero.Transit.Test.Profiles
{
    /// <summary>
    /// A vehicle mock.
    /// </summary>
    public class VehicleMock : Vehicle
    {
        private readonly Func<IAttributeCollection, FactorAndSpeed> _getFactorAndSpeed;
        private readonly string _name;
        private readonly string[] _vehicleTypes;

        /// <summary>
        /// Creates a new mock vehicle.
        /// </summary>
        public VehicleMock(string name, string[] vehicleTypes, Func<IAttributeCollection, FactorAndSpeed> getFactorAndSpeed)
        {
            _name = name;
            _vehicleTypes = vehicleTypes;
            _getFactorAndSpeed = getFactorAndSpeed;


            this.Register(new Profile("shortest", ProfileMetric.DistanceInMeters, this.VehicleTypes, null, this));
            this.Register(new Profile(string.Empty, ProfileMetric.TimeInSeconds, this.VehicleTypes, null, this));
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public override string[] VehicleTypes
        {
            get
            {
                return _vehicleTypes;
            }
        }

        /// <summary>
        /// Gets the name of this vehicle.
        /// </summary>
        public override string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Calculates a factor and speed.
        /// </summary>
        public override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes, Whitelist whitelist)
        {
            return _getFactorAndSpeed(attributes);
        }

        /// <summary>
        /// Creates a mock car.
        /// </summary>
        /// <returns></returns>
        public static VehicleMock Car()
        {
            return new VehicleMock("Car", new string[] { "motor_vehicle", "vehicle" }, (a) =>
            {
                return new FactorAndSpeed()
                {
                    SpeedFactor = 1 / 50f / 3.6f,
                    Value = 1 / 50f / 3.6f,
                    Direction = 0
                };
            });
        }

        /// <summary>
        /// Creates a mock car.
        /// </summary>
        /// <returns></returns>
        public static VehicleMock Car(Func<IAttributeCollection, FactorAndSpeed> getFactorAndSpeed)
        {
            return new VehicleMock("Car", new string[] { "motorcar", "motor_vehicle", "vehicle" }, getFactorAndSpeed);
        }

        /// <summary>
        /// Creates a mock car.
        /// </summary>
        /// <returns></returns>
        public static VehicleMock Mock(string name, Func<IAttributeCollection, FactorAndSpeed> getFactorAndSpeed,
            params string[] vehicleTypes)
        {
            return new VehicleMock(name, vehicleTypes, getFactorAndSpeed);
        }
    }
}
