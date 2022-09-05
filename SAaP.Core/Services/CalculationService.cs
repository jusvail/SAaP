﻿using Microsoft.UI.Xaml.Data;
using SAaP.Core.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAaP.Core.Services
{
    public static class CalculationService
    {
        /// <summary>
        /// keep 2 decimal point
        /// </summary>
        /// <param name="input">input</param>
        /// <returns>result</returns>
        public static double Round2(double input) => Math.Round(input, 2);

        public static double CalcOverprice(double yesterDaysEnding, double todaysHigh) =>
            Round2(100 * (todaysHigh - yesterDaysEnding) / yesterDaysEnding);

        /// <summary>
        /// don't reverse yesterday and today!!!
        /// </summary>
        /// <param name="yesterday">OriginalData of yesterday</param>
        /// <param name="today">OriginalData of today</param>
        /// <returns></returns>
        public static double CalcOverprice(OriginalData yesterday, OriginalData today)
            => CalcOverprice(yesterday.Ending, today.High);
    }
}