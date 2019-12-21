﻿using System;
using System.ComponentModel;
using Mapping_Tools.Classes.MathUtil;

namespace Mapping_Tools.Components.Graph.Interpolation.Interpolators {
    [DisplayName("Parabola")]
    [VerticalMirrorInterpolator]
    public class ParabolaInterpolator : CustomInterpolator, IDerivableInterpolator, IIntegrableInterpolator {
        public string Name => "Parabola";

        public ParabolaInterpolator() {
            InterpolationFunction = Function;
        }

        public double Function(double t, double p) {
            p = MathHelper.Clamp(p, -1, 1);
            return -p * t * t + (p + 1) * t;
        }

        public IGraphInterpolator GetDerivativeInterpolator() {
            return new LinearInterpolator();
        }

        public double GetDerivative(double t) {
            var p = MathHelper.Clamp(P, -1, 1);
            return -2 * p * t + p + 1;
        }

        public IGraphInterpolator GetPrimitiveInterpolator(double x1, double y1, double x2, double y2) {
            return new PrimitiveParabolaInterpolator {P = P};
        }

        public double GetIntegral(double t1, double t2) {
            return Primitive(t2) - Primitive(t1);
        }

        private double Primitive(double t) {
            var p = MathHelper.Clamp(P, -1, 1);
            return 1d / 3 * -p * Math.Pow(t, 3) + 0.5 * (p + 1) * t * t;
        }
    }
}