﻿using System;
using System.Threading.Tasks;
using Statiq.App;
using Statiq.Web;

namespace Blog
{
    class Program
    {
        public static async Task<int> Main(string[] args) => 
            await Bootstrapper
            .Factory
            .CreateWeb(args)
            .RunAsync();
    }
}
