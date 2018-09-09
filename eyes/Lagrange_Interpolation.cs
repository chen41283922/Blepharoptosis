using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeCalibrations
{
    class Lagrange_Interpolation
    {
        public Lagrange_Interpolation() { }

        public double[] zeros(int n)
        {
            double[] array = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                array[i] = 0;
            }
            return array;
        }

        private double denominator(int i, List<PointF> points)
        {
            double result = 1;
            double x_i = points.ElementAt(i).X;
            for (int j = points.Count() - 1; j >= 0; j--)
            {
                if (i != j)
                {
                    result = result * (x_i - points.ElementAt(j).X);
                }
            }
            return result;
        }

        // calculate coefficients for Li polynomial
        public double[] interpolation_polynomial(int i, List<PointF> points)
        {
            double[] coefficients = zeros(points.Count);
            coefficients[0] = ((double)1 / denominator(i, points));
            double[] new_coefficients;

            for (int k = 0; k < points.Count; k++)
            {
                if (k == i)
                {
                    continue;
                }
                new_coefficients = zeros(points.Count);

                for (int j = (k < i) ? k + 1 : k; j >= 0; j--)
                {
                    if (j + 1 < points.Count)
                    {
                        new_coefficients[j + 1] = new_coefficients[j + 1] + coefficients[j];
                    }
                    new_coefficients[j] = new_coefficients[j] - (points.ElementAt(k).X * coefficients[j]);
                }
                coefficients = new_coefficients;
            }
            return coefficients;
        }

        // calculate coefficients of polynomial
        public double[] get_Coefficient(List<PointF> points)
        {
            double[] polynomial = zeros(points.Count());
            double[] coefficients;
            for (int i = 0; i < points.Count(); ++i)
            {
                coefficients = interpolation_polynomial(i, points);
                for (int k = 0; k < points.Count(); ++k)
                {
                    polynomial[k] = polynomial[k] + (points.ElementAt(i).Y * coefficients[k]);
                }
            }
            return polynomial;
        }

    }
}
