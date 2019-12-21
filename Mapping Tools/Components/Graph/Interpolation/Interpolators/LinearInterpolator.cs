﻿using System.ComponentModel;

namespace Mapping_Tools.Components.Graph.Interpolation.Interpolators {
    [IgnoreInterpolator]
    [DisplayName("Linear")]
    public class LinearInterpolator : CustomInterpolator, IDerivableInterpolator, IIntegrableInterpolator {
        public LinearInterpolator() : base((t, p) => t) {}

        public IGraphInterpolator GetDerivativeInterpolator() {
            return new LinearInterpolator();
        }

        public double GetDerivative(double t) {
            return 1;
        }

        public IGraphInterpolator GetPrimitiveInterpolator(double x1, double y1, double x2, double y2) {
            return new ParabolaInterpolator {P = (y1 - y2) / (y1 + y2)};
        }

        public double GetIntegral(double t1, double t2) {
            return 0.5 * t2 * t2 - 0.5 * t1 * t1;
        }
    }
}