using CenterSpace.NMath.Core;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace HeatRod
{
    internal class HeatEquation : DoubleFunctional
    {
        public HeatEquation(int xDimension) : base(xDimension)
        {
        }

        public override double Evaluate(DoubleVector x)
        {
            double result = Calculation.fourierHeat.Keys.ToList()[0].Evaluate(x[0]);
            for(int i = 1; i < Calculation.fourierHeat.Count; i++)
            {
                double L = Calculation.upperX - Calculation.lowerX;
                var baseFunc = Calculation.fourierHeat.Keys.ToList()[i];
                result += baseFunc.Evaluate(x[0]) * Math.Exp(-1 * Calculation.alpha * Math.Pow((Calculation.fourierHeat[baseFunc] * Math.PI) / L, 2) * x[1]);
            }
            return result;
        }
    }
    internal static class Calculation
    {
        public static IEnumerable<T> SliceRow<T>(this T[,] array, int row)
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                yield return array[i, row];
            }
        }

        /// <summary>
        /// A function to get the initial temperature given a position.
        /// </summary>
        internal static OneVariableFunction initialHeat;
        internal static Dictionary<OneVariableFunction, int> fourierHeat;
        internal static HeatEquation fourierTime;

        // Limits
        internal static double alpha;

        internal static double lowerX;
        internal static double lowerY;
        internal static double upperX;
        internal static double upperY;
        internal static int fourierDetail;

        static Calculation()
        {
            alpha = 0.5;
            fourierDetail = 10;
            lowerX = 0;
            upperX = 1;
            lowerY = -1;
            upperY = 1;
            // The initial function for heat given position
            initialHeat = new OneVariableFunction(x => Math.Cos(Math.PI * x));
            fourierHeat = getFourierSeries(initialHeat);
            fourierTime = new HeatEquation(2);
        }

        /// <summary>
        /// Finds the fourier series up to a certain detail of a function
        /// </summary>
        /// <param name="input">The function to find the series of</param>
        /// <returns>A function representing the series.</returns>
        static Dictionary<OneVariableFunction, int> getFourierSeries(OneVariableFunction input)
        {
            double L = upperX - lowerX;
            double constant = (1 / L) * input.Integrate(-1 * L, L);
            var result = new Dictionary<OneVariableFunction, int>();
            result.Add(new OneVariableFunction(x => constant / 2), 0);

            for (int number = 1; number <= fourierDetail; number++)
            {
                int j = number;

                double sinCo = getSineCoeff(input, j, L);
                double cosCo = getCosineCoeff(input, j, L);

                var cosineFunc = new OneVariableFunction(x => getCosineCoeff(input, j, L) * Math.Cos(j * x * Math.PI / L));
                //var sineFunc = new OneVariableFunction(x => getSineCoeff(input, j, L) * Math.Sin(j * x * Math.PI / L));

                result.Add(cosineFunc, j);
                //result.Add(sineFunc, j);
            }
            return result;
        }

        static double getCosineCoeff(OneVariableFunction input, double number, double L)
        {
            var cosineFunction = new OneVariableFunction(theta => Math.Cos(number * Math.PI * theta / L));
            var integrand = input * cosineFunction;
            GaussKronrodIntegrator intg = new GaussKronrodIntegrator();
            var integral = intg.Integrate(integrand, -1*L, L);
            return (1 / L) * integral;
        }

        static double getSineCoeff(OneVariableFunction input, int number, double L)
        {
            OneVariableFunction cosine = new OneVariableFunction(x => input.Evaluate(x) * Math.Sin(number * x * Math.PI / L));
            var sineFunction = new OneVariableFunction(theta => Math.Sin(number * Math.PI * theta / L));
            var integrand = input * sineFunction;
            GaussKronrodIntegrator intg = new GaussKronrodIntegrator();
            double integral = intg.Integrate(integrand, -1 * L, L);
            return (1 / L) * integral;
        }


    }
}
