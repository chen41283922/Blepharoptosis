using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SomeCalibrations
{
    class Parabola
    {
        double[] coefficient;//升冪, 0 based
        int Power;// 1 based
        double a, b, c;
        Lagrange_Interpolation Lagrange;

        public Parabola() { }
        public Parabola(Parabola copy)
        {
            this.coefficient = copy.coefficient;
            this.Power = copy.Power;
            this.Lagrange = copy.Lagrange;
        }

        public Parabola(List<PointF> points) {
            
            coefficient = FindPolynomialLeastSquaresFit(points,3);
            Power = 3;
        }

        public Parabola(PointF c,PointF l,PointF r) {
            double[,] ma = { 
                { Math.Pow(c.X,2), c.X, 1, c.Y }, 
                { Math.Pow(l.X,2), l.X, 1, l.Y }, 
                { Math.Pow(r.X,2), r.X, 1, r.Y } };
            double[] x = new double[3];
            GaussEliminate.Gauss(3,ma,x);
            this.a = x[0];
            this.b = x[1];
            this.c = x[2];
        }
        public Parabola(double a,double b,double c) {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        // Input coordinate X , get coordinate Y
        public double FY(double x)
        {
            Power = coefficient.Count();
            double y = 0;
            while (this.Power > 0) {
                y += coefficient[Power - 1] * Math.Pow(x, Power - 1);
                Power--;
            }
            return y;
        }

        // Input coordinate X , get the 'slope' at X position
        public double DifferentialFY(double x)
        {
            Power = coefficient.Count();
            double y = 0;
            Power = Power - 1;
            while (Power > 0)
            {
                y += Power * coefficient[Power] * Math.Pow(x, Power - 1);
                Power--;
            }
            return y;
        }

        // Input  : upper bound & lower bound , to calculate the area within the 'upper bound' and 'lower bound' by integral
        // Output : Area
        public double Integral(double upper, double lower)
        {
            Power = coefficient.Count();
            double temp_upper = 0;
            double temp_lower = 0;

            while (Power > 0)
            {
                temp_upper += coefficient[Power-1] / Power * Math.Pow(upper, Power);
                temp_lower += coefficient[Power - 1] / Power * Math.Pow(lower, Power);
                Power--;
            }
            return temp_upper - temp_lower;


        }

        

        // Find the least squares linear fit.
        public double[] FindPolynomialLeastSquaresFit(List<PointF> points, int degree)
        {
            // Allocate space for (degree + 1) equations with 
            // (degree + 2) terms each (including the constant term).
            double[,] coeffs = new double[degree + 1, degree + 2];

            // Calculate the coefficients for the equations.
            for (int j = 0; j <= degree; j++)
            {
                // Calculate the coefficients for the jth equation.

                // Calculate the constant term for this equation.
                coeffs[j, degree + 1] = 0;
                foreach (PointF pt in points)
                {
                    coeffs[j, degree + 1] -= Math.Pow(pt.X, j) * pt.Y;
                }

                // Calculate the other coefficients.
                for (int a_sub = 0; a_sub <= degree; a_sub++)
                {
                    // Calculate the dth coefficient.
                    coeffs[j, a_sub] = 0;
                    foreach (PointF pt in points)
                    {
                        coeffs[j, a_sub] -= Math.Pow(pt.X, a_sub + j);
                    }
                }
            }

            // Solve the equations.
            double[] answer = GaussianElimination(coeffs);

            return answer;
        }

        // Perform Gaussian elimination on these coefficients.
        // Return the array of values that gives the solution.
        private double[] GaussianElimination(double[,] coeffs)
        {
            int max_equation = coeffs.GetUpperBound(0);
            int max_coeff = coeffs.GetUpperBound(1);
            for (int i = 0; i <= max_equation; i++)
            {
                // Use equation_coeffs[i, i] to eliminate the ith
                // coefficient in all of the other equations.

                // Find a row with non-zero ith coefficient.
                if (coeffs[i, i] == 0)
                {
                    for (int j = i + 1; j <= max_equation; j++)
                    {
                        // See if this one works.
                        if (coeffs[j, i] != 0)
                        {
                            // This one works. Swap equations i and j.
                            // This starts at k = i because all
                            // coefficients to the left are 0.
                            for (int k = i; k <= max_coeff; k++)
                            {
                                double temp = coeffs[i, k];
                                coeffs[i, k] = coeffs[j, k];
                                coeffs[j, k] = temp;
                            }
                            break;
                        }
                    }
                }

                // Make sure we found an equation with
                // a non-zero ith coefficient.
                double coeff_i_i = coeffs[i, i];
                if (coeff_i_i == 0)
                {
                    throw new ArithmeticException(String.Format(
                        "There is no unique solution for these points.",
                        coeffs.GetUpperBound(0) - 1));
                }

                // Normalize the ith equation.
                for (int j = i; j <= max_coeff; j++)
                {
                    coeffs[i, j] /= coeff_i_i;
                }

                // Use this equation value to zero out
                // the other equations' ith coefficients.
                for (int j = 0; j <= max_equation; j++)
                {
                    // Skip the ith equation.
                    if (j != i)
                    {
                        // Zero the jth equation's ith coefficient.
                        double coef_j_i = coeffs[j, i];
                        for (int d = 0; d <= max_coeff; d++)
                        {
                            coeffs[j, d] -= coeffs[i, d] * coef_j_i;
                        }
                    }
                }
            }

            // At this point, the ith equation contains
            // 2 non-zero entries:
            //      The ith entry which is 1
            //      The last entry coeffs[max_coeff]
            // This means Ai = equation_coef[max_coeff].
            double[] solution = new double[max_equation + 1];
            for (int i = 0; i <= max_equation; i++)
            {
                solution[i] = coeffs[i, max_coeff];
            }

            // Return the solution values.
            return solution;
        }

    }
}
